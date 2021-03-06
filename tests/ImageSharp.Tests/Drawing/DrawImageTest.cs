﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Tests.TestUtilities.ImageComparison;
using SixLabors.Primitives;
using Xunit;

namespace SixLabors.ImageSharp.Tests
{
    [GroupOutput("Drawing")]
    public class DrawImageTest : FileTestBase
    {
        private const PixelTypes PixelTypes = Tests.PixelTypes.Rgba32;

        public static readonly string[] TestFiles = {
               TestImages.Jpeg.Baseline.Calliphora,
               TestImages.Bmp.Car,
               TestImages.Png.Splash,
               TestImages.Gif.Rings
        };

        [Theory]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Normal)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Multiply)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Add)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Subtract)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Screen)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Darken)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Lighten)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Overlay)]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.HardLight)]
        public void ImageShouldApplyDrawImage<TPixel>(TestImageProvider<TPixel> provider, PixelColorBlendingMode mode)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            using (var blend = Image.Load<TPixel>(TestFile.Create(TestImages.Bmp.Car).Bytes))
            {
                blend.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
                image.Mutate(x => x.DrawImage(blend, new Point(image.Width / 4, image.Height / 4), mode, .75f));
                image.DebugSave(provider, new { mode });
            }
        }

        [Theory]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Normal)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Multiply)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Add)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Subtract)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Screen)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Darken)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Lighten)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.Overlay)]
        [WithFile(TestImages.Png.Rainbow, PixelTypes, PixelColorBlendingMode.HardLight)]
        public void ImageBlendingMatchesSvgSpecExamples<TPixel>(TestImageProvider<TPixel> provider, PixelColorBlendingMode mode)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> background = provider.GetImage())
            using (var source = Image.Load<TPixel>(TestFile.Create(TestImages.Png.Ducky).Bytes))
            {
                background.Mutate(x => x.DrawImage(source, mode, 1F));
                VerifyImage(provider, mode, background);
            }
        }

        [Theory]
        [WithFileCollection(nameof(TestFiles), PixelTypes, PixelColorBlendingMode.Normal)]
        public void ImageShouldDrawTransformedImage<TPixel>(TestImageProvider<TPixel> provider, PixelColorBlendingMode mode)
            where TPixel : struct, IPixel<TPixel>
        {
            using (Image<TPixel> image = provider.GetImage())
            using (var blend = Image.Load<TPixel>(TestFile.Create(TestImages.Bmp.Car).Bytes))
            {
                AffineTransformBuilder builder = new AffineTransformBuilder()
                    .AppendRotationDegrees(45F)
                    .AppendScale(new SizeF(.25F, .25F))
                    .AppendTranslation(new PointF(10, 10));

                // Apply a background color so we can see the translation.
                blend.Mutate(x => x.Transform(builder));
                blend.Mutate(x => x.BackgroundColor(Color.HotPink));

                // Lets center the matrix so we can tell whether any cut-off issues we may have belong to the drawing processor
                var position = new Point((image.Width - blend.Width) / 2, (image.Height - blend.Height) / 2);
                image.Mutate(x => x.DrawImage(blend, position, mode, .75F));
                image.DebugSave(provider, new[] { "Transformed" });
            }
        }

        [Theory]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32)]
        public void ImageShouldHandleNegativeLocation(TestImageProvider<Rgba32> provider)
        {
            using (Image<Rgba32> background = provider.GetImage())
            using (var overlay = new Image<Rgba32>(50, 50))
            {
                overlay.Mutate(x => x.Fill(Rgba32.Black));

                const int xy = -25;
                Rgba32 backgroundPixel = background[0, 0];
                Rgba32 overlayPixel = overlay[Math.Abs(xy) + 1, Math.Abs(xy) + 1];

                background.Mutate(x => x.DrawImage(overlay, new Point(xy, xy), PixelColorBlendingMode.Normal, 1F));

                Assert.Equal(Rgba32.White, backgroundPixel);
                Assert.Equal(overlayPixel, background[0, 0]);

                background.DebugSave(provider, testOutputDetails: "Negative");
            }
        }

        [Theory]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32)]
        public void ImageShouldHandlePositiveLocation(TestImageProvider<Rgba32> provider)
        {
            using (Image<Rgba32> background = provider.GetImage())
            using (var overlay = new Image<Rgba32>(50, 50))
            {
                overlay.Mutate(x => x.Fill(Rgba32.Black));

                const int xy = 25;
                Rgba32 backgroundPixel = background[xy - 1, xy - 1];
                Rgba32 overlayPixel = overlay[0, 0];

                background.Mutate(x => x.DrawImage(overlay, new Point(xy, xy), PixelColorBlendingMode.Normal, 1F));

                Assert.Equal(Rgba32.White, backgroundPixel);
                Assert.Equal(overlayPixel, background[xy, xy]);

                background.DebugSave(provider, testOutputDetails: "Positive");
            }
        }
        [Theory]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32)]
        public void ImageShouldHandlePositiveLocationTruncatedOverlay(TestImageProvider<Rgba32> provider)
        {
            using (Image<Rgba32> background = provider.GetImage())
            using (var overlay = new Image<Rgba32>(50, 50))
            {
                overlay.Mutate(x => x.Fill(Rgba32.Black));

                const int xy = 75;
                Rgba32 backgroundPixel = background[xy - 1, xy - 1];
                Rgba32 overlayPixel = overlay[0, 0];

                background.Mutate(x => x.DrawImage(overlay, new Point(xy, xy), PixelColorBlendingMode.Normal, 1F));

                Assert.Equal(Rgba32.White, backgroundPixel);
                Assert.Equal(overlayPixel, background[xy, xy]);

                background.DebugSave(provider, testOutputDetails: "PositiveTruncated");
            }
        }

        [Theory]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32, -30, -30)]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32, 130, -30)]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32, 130, 130)]
        [WithSolidFilledImages(100, 100, 255, 255, 255, PixelTypes.Rgba32, -30, 130)]
        public void NonOverlappingImageThrows(TestImageProvider<Rgba32> provider, int x, int y)
        {
            using (Image<Rgba32> background = provider.GetImage())
            using (var overlay = new Image<Rgba32>(Configuration.Default, 10, 10, Rgba32.Black))
            {
                ImageProcessingException ex = Assert.Throws<ImageProcessingException>(Test);

                Assert.Contains("does not overlap", ex.ToString());

                void Test()
                {
                    background.Mutate(context => context.DrawImage(overlay, new Point(x, y), GraphicsOptions.Default));
                }
            }
        }

        private static void VerifyImage<TPixel>(
            TestImageProvider<TPixel> provider,
            PixelColorBlendingMode mode,
            Image<TPixel> img)
            where TPixel : struct, IPixel<TPixel>
        {
            img.DebugSave(
                provider,
                new { mode },
                appendPixelTypeToFileName: false,
                appendSourceFileOrDescription: false);

            var comparer = ImageComparer.TolerantPercentage(0.01F, 3);
            img.CompareFirstFrameToReferenceOutput(comparer,
                provider,
                new { mode },
                appendPixelTypeToFileName: false,
                appendSourceFileOrDescription: false);
        }
    }
}