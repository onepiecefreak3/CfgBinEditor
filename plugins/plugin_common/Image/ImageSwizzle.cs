using System.Runtime.CompilerServices;
using Kanvas.Contract;
using Kanvas.Contract.DataClasses;
using plugin_common.Font.Enums;
using SixLabors.ImageSharp;

namespace plugin_common.Image
{
    /// <summary>
    /// Pixel swizzle for Level-5 images.
    /// Each layout is the straight-line form of the IL <c>MasterSwizzle</c> emits for that bit field.
    /// </summary>
    internal class ImageSwizzle : IImageSwizzle
    {
        private readonly ImageSwizzleLayout _layout;
        private readonly int _widthInTiles;

        /// <inheritdoc />
        public int Width { get; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int MacroTileWidth { get; }

        /// <inheritdoc />
        public int MacroTileHeight { get; }

        public ImageSwizzle(SwizzleOptions options, PlatformType platform)
        {
            Width = options.Size.Width + 7 & ~7;
            Height = options.Size.Height + 7 & ~7;

            (_layout, MacroTileWidth, MacroTileHeight) = ResolveLayout(options, platform);
            _widthInTiles = (Width + MacroTileWidth - 1) / MacroTileWidth;
        }

        /// <inheritdoc />
        public Point Transform(Point point) => Get(point.Y * Width + point.X);

        /// <inheritdoc />
        public Point Get(int pointCount)
        {
            return _layout switch
            {
                ImageSwizzleLayout.Ctr => GetCtr(pointCount),
                ImageSwizzleLayout.Tile32X8 => GetTile32X8(pointCount),
                ImageSwizzleLayout.Tile16X8 => GetTile16X8(pointCount),
                ImageSwizzleLayout.Tile8X8 => GetTile8X8(pointCount),
                ImageSwizzleLayout.Tile4X8 => GetTile4X8(pointCount),
                ImageSwizzleLayout.Tile16X4 => GetTile16X4(pointCount),
                _ => throw new InvalidOperationException("Unknown image swizzle.")
            };
        }

        private static (ImageSwizzleLayout Layout, int MacroTileWidth, int MacroTileHeight) ResolveLayout(
            SwizzleOptions options,
            PlatformType platform)
        {
            return platform switch
            {
                PlatformType.Ctr => (ImageSwizzleLayout.Ctr, 8, 8),
                PlatformType.Psp => ResolvePspLayout(options),
                _ => options.EncodingInfo.ColorsPerValue > 1
                    ? (ImageSwizzleLayout.Tile16X4, 16, 4)
                    : (ImageSwizzleLayout.Tile8X8, 8, 8)
            };
        }

        private static (ImageSwizzleLayout Layout, int MacroTileWidth, int MacroTileHeight) ResolvePspLayout(
            SwizzleOptions options)
        {
            return options.EncodingInfo.BitsPerValue switch
            {
                0x04 => (ImageSwizzleLayout.Tile32X8, 32, 8),
                0x08 => (ImageSwizzleLayout.Tile16X8, 16, 8),
                0x10 => (ImageSwizzleLayout.Tile8X8, 8, 8),
                0x20 => (ImageSwizzleLayout.Tile4X8, 4, 8),
                _ => throw new InvalidOperationException("Unknown swizzle for platform 'P'.")
            };
        }

        /// <summary>
        /// Bit field (0, 1), (1, 0), (0, 2), (2, 0), (0, 4), (4, 0).
        /// </summary>
        private Point GetCtr(int pointCount)
        {
            (int x, int y) = GetMacroOrigin(pointCount, 8, 8);

            x ^= Bit(pointCount, 1, 1);
            x ^= Bit(pointCount, 3, 2);
            x ^= Bit(pointCount, 5, 4);

            y ^= Bit(pointCount, 0, 1);
            y ^= Bit(pointCount, 2, 2);
            y ^= Bit(pointCount, 4, 4);

            return new Point(x, y);
        }

        /// <summary>
        /// Bit field (1, 0), (2, 0), (4, 0), (8, 0), (16, 0), (0, 1), (0, 2), (0, 4).
        /// </summary>
        private Point GetTile32X8(int pointCount)
        {
            (int x, int y) = GetMacroOrigin(pointCount, 32, 8);

            x ^= Bit(pointCount, 0, 1);
            x ^= Bit(pointCount, 1, 2);
            x ^= Bit(pointCount, 2, 4);
            x ^= Bit(pointCount, 3, 8);
            x ^= Bit(pointCount, 4, 16);

            y ^= Bit(pointCount, 5, 1);
            y ^= Bit(pointCount, 6, 2);
            y ^= Bit(pointCount, 7, 4);

            return new Point(x, y);
        }

        /// <summary>
        /// Bit field (1, 0), (2, 0), (4, 0), (8, 0), (0, 1), (0, 2), (0, 4).
        /// </summary>
        private Point GetTile16X8(int pointCount)
        {
            (int x, int y) = GetMacroOrigin(pointCount, 16, 8);

            x ^= Bit(pointCount, 0, 1);
            x ^= Bit(pointCount, 1, 2);
            x ^= Bit(pointCount, 2, 4);
            x ^= Bit(pointCount, 3, 8);

            y ^= Bit(pointCount, 4, 1);
            y ^= Bit(pointCount, 5, 2);
            y ^= Bit(pointCount, 6, 4);

            return new Point(x, y);
        }

        /// <summary>
        /// Bit field (1, 0), (2, 0), (4, 0), (0, 1), (0, 2), (0, 4).
        /// </summary>
        private Point GetTile8X8(int pointCount)
        {
            (int x, int y) = GetMacroOrigin(pointCount, 8, 8);

            x ^= Bit(pointCount, 0, 1);
            x ^= Bit(pointCount, 1, 2);
            x ^= Bit(pointCount, 2, 4);

            y ^= Bit(pointCount, 3, 1);
            y ^= Bit(pointCount, 4, 2);
            y ^= Bit(pointCount, 5, 4);

            return new Point(x, y);
        }

        /// <summary>
        /// Bit field (1, 0), (2, 0), (0, 1), (0, 2), (0, 4).
        /// </summary>
        private Point GetTile4X8(int pointCount)
        {
            (int x, int y) = GetMacroOrigin(pointCount, 4, 8);

            x ^= Bit(pointCount, 0, 1);
            x ^= Bit(pointCount, 1, 2);

            y ^= Bit(pointCount, 2, 1);
            y ^= Bit(pointCount, 3, 2);
            y ^= Bit(pointCount, 4, 4);

            return new Point(x, y);
        }

        /// <summary>
        /// Bit field (1, 0), (2, 0), (0, 1), (0, 2), (4, 0), (8, 0).
        /// </summary>
        private Point GetTile16X4(int pointCount)
        {
            (int x, int y) = GetMacroOrigin(pointCount, 16, 4);

            x ^= Bit(pointCount, 0, 1);
            x ^= Bit(pointCount, 1, 2);
            x ^= Bit(pointCount, 4, 4);
            x ^= Bit(pointCount, 5, 8);

            y ^= Bit(pointCount, 2, 1);
            y ^= Bit(pointCount, 3, 2);

            return new Point(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int X, int Y) GetMacroOrigin(int pointCount, int macroTileWidth, int macroTileHeight)
        {
            uint macroTileCount = (uint)pointCount / (uint)(macroTileWidth * macroTileHeight);
            int macroX = (int)(macroTileCount % (uint)_widthInTiles);
            int macroY = (int)(macroTileCount / (uint)_widthInTiles);

            return (macroX * macroTileWidth, macroY * macroTileHeight);
        }

        /// <summary>
        /// ((pointCount >> index) % 2) * coordinate, matching the emitted Shr_Un / Rem_Un / Mul sequence.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Bit(int pointCount, int index, int coordinate)
        {
            return (int)(((uint)pointCount >> index) % 2) * coordinate;
        }

        private enum ImageSwizzleLayout
        {
            Ctr,
            Tile32X8,
            Tile16X8,
            Tile8X8,
            Tile4X8,
            Tile16X4
        }
    }
}
