﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace SixLabors.ImageSharp.Tests.Processing.Processors.Convolution
{
    [GroupOutput("Convolution")]
    public class BoxBlurTest : Basic1ParameterConvolutionTests
    {
        protected override void Apply<TPixel>(IImageProcessingContext<TPixel> ctx, int value) => ctx.BoxBlur(value);

        protected override void Apply<TPixel>(IImageProcessingContext<TPixel> ctx, int value, Rectangle bounds) =>
            ctx.BoxBlur(value, bounds);
    }
}