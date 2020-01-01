﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nucleus.Diagnostics {
    public struct LogData {
        public string String { get; set; }
        public ConsoleColor Color { get; set; }
        public OutputLevel OutputLevel { get; set; }

        public LogData(string str, ConsoleColor color, OutputLevel displayLevel) {
            String = str;
            Color = color;
            OutputLevel = displayLevel;
        }
    }
}
