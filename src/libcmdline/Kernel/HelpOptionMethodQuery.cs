﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLine.Kernel
{
    internal sealed class HelpOptionMethodQuery : IMemberQuery
    {
        public IEnumerable<IMember> SelectMembers(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            yield return (from mi in type.GetMethods()
                   let attributes = mi.GetCustomAttributes(typeof(HelpOptionAttribute), true)
                   where attributes.Length > 0
                   select new HelpOptionMethod(mi, (HelpOptionAttribute)attributes.ElementAt(0)) as IMember).SingleOrDefault();
        }
    }
}