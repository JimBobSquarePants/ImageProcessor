namespace ImageProcessorCore.Tests
{
    using Xunit;

    public class ColorSpacialTransformTests
    {
        public class DarkenTests
        {
            [Fact]
            
            public void DarkenBlendConvertsToDarkerRedColorChannel()
            {
                var darkerRed = 25;
                var backdrop = new Color(50, 0, 0);
                var source = new Color(darkerRed, 0, 0);

                var result = Color.Darken(backdrop, source);

                Assert.Equal(result.R, darkerRed);
            }
            [Fact]
            
            public void DarkenBlendConvertsTodarkerGreenColorChannel()
            {
                var darkerGreen = 100;
                var backdrop = new Color(0, darkerGreen, 0);
                var source = new Color(0, 130, 0);

                var result = Color.Darken(backdrop, source);

                Assert.Equal(result.G, darkerGreen);
            }
            [Fact]
            
            public void DarkenBlendConvertsTodarkerBlueColorChannel()
            {
                var darkerBlue = 8;
                var backdrop = new Color(0, 0, darkerBlue);
                var source = new Color(0, 0, 30);

                var result = Color.Darken(backdrop, source);

                Assert.Equal(result.B, darkerBlue);
            }
            [Fact]
            
            public void DarkenBlendConvertsAllChannelsTodarkerColor()
            {
                var darkerBlue = 10;
                var darkerRed = 0;
                var darkerGreen = 186;
                var backdrop = new Color(darkerRed, 189, darkerBlue);
                var source = new Color(120, darkerGreen, 30);

                var result = Color.Darken(backdrop, source);

                Assert.Equal(result.R, darkerRed);
                Assert.Equal(result.G, darkerGreen);
                Assert.Equal(result.B, darkerBlue);
            }
        }
        public class LightenTests
        {
            [Fact]
            
            public void LightenBlendConvertsToLighterRedColorChannel()
            {
                var lighterRed = 150;
                var backdrop = new Color(50, 0, 0);
                var source = new Color(lighterRed, 0, 0);

                var result = Color.Lighten(backdrop, source);

                Assert.Equal(result.R, lighterRed);
            }
            [Fact]
            
            public void LightenBlendConvertsToLighterGreenColorChannel()
            {
                var lighterGreen = 200;
                var backdrop = new Color(0, lighterGreen, 0);
                var source = new Color(0, 130, 0);

                var result = Color.Lighten(backdrop, source);

                Assert.Equal(result.G, lighterGreen);
            }
            [Fact]
            
            public void LightenBlendConvertsToLighterBlueColorChannel()
            {
                var lighterBlue = 190;
                var backdrop = new Color(0, 0, lighterBlue);
                var source = new Color(0, 0, 30);

                var result = Color.Lighten(backdrop, source);

                Assert.Equal(result.B, lighterBlue);
            }
            [Fact]
            
            public void LightenBlendConvertsAllChannelsToLighterColor()
            {
                var lighterBlue = 120;
                var lighterRed = 250;
                var lighterGreen = 190;
                var backdrop = new Color(lighterRed, 189, lighterBlue);
                var source = new Color(0, lighterGreen, 30);

                var result = Color.Lighten(backdrop, source);

                Assert.Equal(result.R, lighterRed);
                Assert.Equal(result.G, lighterGreen);
                Assert.Equal(result.B, lighterBlue);
            }
        }

        public class MultiplyTests
        {
            [Fact]
            public void MultiplyBlendConvertsRedBackdropAndGreenOverlayToBlack()
            {
                var backdrop = Color.Red;
                var overlay = Color.Green;

                var result = Color.Multiply(backdrop, overlay);

                Assert.Equal(Color.Black, result);
            }
            [Fact]
            public void MultiplyBlendConvertsBlueBackdropAndWhiteOverlayToBlue()
            {
                var backdrop = Color.Blue;
                var overlay = Color.White;

                var result = Color.Multiply(backdrop, overlay);

                Assert.Equal(Color.Blue, result);
            }
            [Fact]
            public void MultiplyBlendConvertsBlueBackdropAndBlackOverlayToBlack()
            {
                var backdrop = Color.Blue;
                var overlay = Color.Black;

                var result = Color.Multiply(backdrop, overlay);

                Assert.Equal(Color.Black, result);
            }
            [Fact]
            public void MultiplyBlendConvertsBlueBackdropAndGrayOverlayToBlueBlack()
            {
                var backdrop = Color.Blue;
                var overlay = Color.Gray;

                var result = Color.Multiply(backdrop, overlay);

                var expected = new Color(0, 0, 0.5f, 1);

                Assert.True(expected.AlmostEquals(result,.01f));
            }
        }
    }
}