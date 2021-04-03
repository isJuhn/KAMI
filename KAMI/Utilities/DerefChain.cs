using System;
using System.Collections.Generic;
using System.Text;

namespace KAMI.Utilities
{
    public class DerefChain
    {
        private IntPtr m_ipc;
        private DerefChain m_parent;
        // TODO: Is this really the right structure for this? We might never care about having children offsets like this.
        private Dictionary<long, DerefChain> m_children = new Dictionary<long, DerefChain>();
        private long m_offset;
        public long Value { get; private set; }

        public DerefChain(IntPtr ipc, long offset, DerefChain parent)
        {
            m_ipc = ipc;
            m_offset = offset;
            m_parent = parent;
            Recalculate();
            if (m_parent != null)
            {
                m_parent.m_children[m_offset] = this;
            }
        }

        public DerefChain(IntPtr ipc, long address) : this(ipc, address, null)
        {
        }

        public DerefChain Chain(long offset)
        {
            return new DerefChain(m_ipc, offset, this);
        }

        public static DerefChain CreateDerefChain(IntPtr ipc, long address, params long[] offsets)
        {
            DerefChain current = new DerefChain(ipc, address);
            foreach (var offset in offsets)
            {
                current = new DerefChain(ipc, offset, current);
            }
            return current;
        }

        public bool Verify()
        {
            return VerifyChains(this);
        }

        public static bool VerifyChains(params DerefChain[] derefChains)
        {
            HashSet<DerefChain> visited = new HashSet<DerefChain>();
            Dictionary<DerefChain, int> recalculationDistance = new Dictionary<DerefChain, int>();
            foreach (var derefChain in derefChains)
            {
                int distance = -1;
                bool valid = true;
                DerefChain current = derefChain;
                while (true)
                {
                    visited.Add(current);
                    distance++;
                    if (!current.VerifyInternal())
                    {
                        valid = false;
                        distance = 0;
                    }
                    if (PCSX2IPC.GetError(current.m_ipc) != PCSX2IPC.IPCStatus.Success)
                    {
                        return false;
                    }
                    if (current.m_parent == null || visited.Contains(current.m_parent))
                    {
                        break;
                    }
                    current = current.m_parent;
                }
                if (!valid)
                {
                    if (recalculationDistance.TryGetValue(current, out int oldDistance))
                    {
                        if (oldDistance > distance)
                        {
                            recalculationDistance[current] = distance;
                        }
                    }
                    else
                    {
                        recalculationDistance[current] = distance;
                    }
                }
            }
            // TODO: Find a better way to traverse this structure to avoid recalculating nodes already recalculated.
            // Alternatively, mark up which nodes will already be recalculated because of their parents in some smart way.
            // Or, collect the nodes to be recalculated in some other kind of structure earlier that keeps the order and don't do
            // recursive recalculation in RecalculateInternal
            foreach (var kvp in recalculationDistance)
            {
                if (!kvp.Key.RecalculateInternal(kvp.Value))
                {
                    return false;
                }
            }
            return true;
        }

        private bool VerifyInternal()
        {
            long actual = m_parent != null ? IPCUtils.ReadU32(m_ipc, (uint)m_parent.Value) + m_offset : m_offset;
            return actual == Value;
        }

        public bool Recalculate()
        {
            return RecalculateInternal(0);
        }

        private bool RecalculateInternal(int numSkipChains)
        {
            if (numSkipChains < 1)
            {
                Value = m_parent != null ? IPCUtils.ReadU32(m_ipc, (uint)m_parent.Value) + m_offset : m_offset;
                if (PCSX2IPC.GetError(m_ipc) != PCSX2IPC.IPCStatus.Success)
                {
                    return false;
                }
            }
            foreach (var child in m_children.Values)
            {
                if (!child.RecalculateInternal(numSkipChains > 0 ? numSkipChains - 1 : 0))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
