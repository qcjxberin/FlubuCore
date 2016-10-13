﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FlubuCore.IO.Wrappers
{
    /// <summary>
    /// Wrapper interface for <see cref="File"/>
    /// </summary>
    public interface IFileWrapper
    {
        bool Exists(string path);

        string ReadAllText(string path);
    }
}
