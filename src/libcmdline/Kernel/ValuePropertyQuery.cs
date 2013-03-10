﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommandLine.Kernel
{
    internal sealed class ValuePropertyQuery : IPropertyQuery
    {
        public IEnumerable<IProperty> SelectProperties(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return from pi in type.GetProperties()
                   let attributes = pi.GetCustomAttributes(typeof(IOptionAttribute), true)
                   where attributes.Length == 1
                   select new OptionProperty(pi, (IOptionAttribute)attributes.ElementAt(0)) as IProperty;
        }
    }
}