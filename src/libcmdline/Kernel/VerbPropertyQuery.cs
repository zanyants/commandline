﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLine.Kernel
{
    internal sealed class VerbPropertyQuery : IMemberQuery
    {
        public IEnumerable<IMember> SelectMembers(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return from pi in type.GetProperties()
                   let attributes = pi.GetCustomAttributes(typeof(VerbAttribute), true)
                   where attributes.Length == 1
                   select new VerbProperty(pi, (VerbAttribute)attributes.ElementAt(0)) as IMember;
        }
    }
}