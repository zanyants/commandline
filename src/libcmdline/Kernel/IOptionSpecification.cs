﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLine.Kernel
{
    internal interface IOptionSpecification
    {
        bool IsSatisfiedBy(string name);
    }
}
