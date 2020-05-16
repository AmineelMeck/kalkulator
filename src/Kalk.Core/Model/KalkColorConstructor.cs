﻿using System;
using Scriban;
using Scriban.Helpers;
using Scriban.Parsing;
using Scriban.Syntax;

namespace Kalk.Core
{
    public abstract class KalkColorConstructor : KalkVectorConstructor<int>
    {
        protected KalkColorConstructor(int dimension) : base(dimension)
        {
        }

        protected override int GetLength(object arg)
        {
            if (arg is string)
            {
                return Dimension;
            }
            return base.GetLength(arg);
        }

        protected abstract override KalkVector<int> NewVector(int dimension);


        protected override int GetArgumentValue(TemplateContext context, SourceSpan span, object arg)
        {
            switch (arg)
            {
                case float f32:
                    return (int)(255 * Math.Clamp(f32, 0.0f, 1.0f));
                case double f64:
                    return (int)(255 * Math.Clamp(f64, 0.0, 1.0));
                case decimal dec:
                    return (int)(255 * Math.Clamp(dec, 0.0m, 1.0m));
                default:
                    return base.GetArgumentValue(context, span, arg);
            }
        }

        protected override void ProcessSingleArgument(TemplateContext context, SourceSpan span, ref int index, object arg, KalkVector<int> vector)
        {
            int value;
            switch (arg)
            {
                case string rgbStr:
                    try
                    {
                        value = int.Parse(rgbStr.TrimStart('#'), System.Globalization.NumberStyles.HexNumber);
                    }
                    catch
                    {
                        throw new ScriptArgumentException(0, $"Expecting an hexadecimal rgb string (e.g #FF80C2) instead of {rgbStr}");
                    }
                    break;
                default:
                    value = GetArgumentValue(context, span, arg);
                    break;
            }

            if (Dimension == 4)
            {
                vector[index++] = (value >> 24) & 0xFF;
            }
            vector[index++] = (value >> 16) & 0xFF;
            vector[index++] = (value >> 8) & 0xFF;
            vector[index++] = value & 0xFF;
        }
    }
}