---
layout: article
title: About KAMI
description: The story behind Kot and Mouse Injector, its development, and its MIT license.
eyebrow: A cat and a mouse
permalink: /about/
---

**KAMI** stands for **Kot and Mouse Injector**, a play on cat and mouse. It brings mouse input to specific emulated games so they can feel more comfortable to play with a keyboard and mouse.

KAMI communicates with RPCS3 and PCSX2 through PINE and uses game-specific handlers to control camera movement. Support depends on the game's title ID, version, and sometimes its game mode. The [compatibility page]({{ '/compatibility/' | relative_url }}) describes what is implemented and what remains uncertain.

## A short history

- **2021:** The [first public commit](https://github.com/isJuhn/KAMI/commit/dfafd9f95efe9469b47926af6b7ac45ecd354554) landed in March, followed by [v0.0.1](https://github.com/isJuhn/KAMI/releases/tag/v0.0.1) in April. [v0.0.2](https://github.com/isJuhn/KAMI/releases/tag/v0.0.2) separated its core library from the WPF interface.
- **2022:** [v0.0.4](https://github.com/isJuhn/KAMI/releases/tag/v0.0.4) improved Insomniac game handling and documented IPC support in mainline RPCS3, removing the need for a custom emulator build.
- **2023:** [v0.0.5](https://github.com/isJuhn/KAMI/releases/tag/v0.0.5) added a configuration file, experimental raw input, Black support, and experimental Battlefield: Bad Company support. [v0.0.6](https://github.com/isJuhn/KAMI/releases/tag/v0.0.6) added Metal Gear Solid 4.
- **2025–2026:** [v0.0.7](https://github.com/isJuhn/KAMI/releases/tag/v0.0.7) restored PCSX2 support and added Killzone 2 title IDs. [v0.0.8](https://github.com/isJuhn/KAMI/releases/tag/v0.0.8) added GoldenEye 007: Reloaded and moved to .NET 10.

The [release history]({{ site.links.releases }}) records published changes. Development builds can include newer work that has not reached a release yet. Thanks to everyone who has contributed code, testing, game information, and bug reports.

## License

KAMI is free and open source under the MIT license. You may use, modify, and redistribute it under the terms below. The [repository license](https://github.com/isJuhn/KAMI/blob/master/LICENSE) is the authoritative copy; bundled dependencies retain their own licenses.

<div class="license-text" markdown="1">

MIT License

Copyright (c) 2021 Johan "isJuhn" Hellgren

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

</div>
