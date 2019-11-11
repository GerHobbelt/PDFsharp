using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using PdfSharp.Drawing;
using PdfSharp.Drawing.BarCodes;

namespace PdfSharp.Drawing.BarCodes
{
    /// <summary>A Class to be able to render a Code 128 bar code</summary>
    /// <remarks>For a more detailed explanation of the Code 128, please visit the following web sites:
    /// http://www.barcodeman.com/info/c128.php3
    /// http://www.adams1.com/128code.html 
    /// https://sv.wikipedia.org/wiki/Code_128
    /// 
    /// ASCII:
    /// http://www.asciitable.com/
    /// https://www.asciitabell.se/
    /// </remarks>
    public class Code128 : BarCode
    {
        /// <summary>A static place holder for the patterns to draw the code 128 barcode</summary>
        public static Dictionary<byte, byte[]> Patterns = null;
        private Code128CodeType Code128Code = Code128CodeType.CodeB;
        private string BarCode = string.Empty;
        private const char CodeC = (char)99;
        private const char CodeB = (char)100;
        private const char CodeA = (char)101;
        private const char FNC1 = (char)102;
        private const int STOPCODE = 106;

        /// <summary>Constructor</summary>
        /// <param name="text">String - The text to be coded</param>
        /// <param name="size">XSize - The size of the bar code</param>
        /// <param name="direction">CodeDirection - Indicates the direction to draw the bar code</param>
        public Code128(string text, XSize size, CodeDirection direction)
        : this(text, size, direction, Code128CodeType.CodeB, false)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="text">String - The text to be coded</param>
        /// <param name="size">XSize - The size of the bar code</param>
        /// <param name="direction">CodeDirection - Indicates the direction to draw the bar code</param>
        /// <param name="isUCC">Set barcode to GS1 EAN/UCC</param>
        public Code128(string text, XSize size, CodeDirection direction, bool isUCC)
        : this(text, size, direction, Code128CodeType.Auto, isUCC)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="text">String - The text to be coded</param>
        /// <param name="size">XSize - The size of the bar code</param>
        /// <param name="direction">CodeDirection - Indicates the direction to draw the bar code</param>
        /// <param name="code128Code">Code_128_Code_Types - Indicates which of the codes to use when rendering the bar code.
        public Code128(string text, XSize size, CodeDirection direction, Code128CodeType code128Code)
        : this(text, size, direction, code128Code, false)
        {
        }

        /// <summary>Constructor</summary>
        /// <param name="text">String - The text to be coded</param>
        /// <param name="size">XSize - The size of the bar code</param>
        /// <param name="direction">CodeDirection - Indicates the direction to draw the bar code</param>
        /// <param name="code128Code">Code_128_Code_Types - Indicates which of the codes to use when rendering the bar code.
        /// The options are A, B, or buffer.</param>
        public Code128(string text, XSize size, CodeDirection direction, Code128CodeType code128Code, bool isUCC)
        : this(null, text, size, direction, code128Code)
        {
            this.Text = text;
            text = isUCC ? PrepareGS1Text(text) : text;

            if (Code128Code == Code128CodeType.CodeC)
            {
                // Ensure that the text is an even length
                //if ((text.Length % 2) == 1) throw new ArgumentOutOfRangeException("Parameter text (string) must have an even length for Code 128 - Code C");
                if ((text.Length % 2) == 1)
                    text = "0" + text;
            }

            BarCode = ParseTextToBarCode(text);

            CheckValues();
        }

        /// <summary>Constructor</summary>
        /// <param name="values">byte[] - The values to be rendered</param>
        /// <param name="text">string - The text to be rendered</param>
        /// <param name="size">XSize - The size of the bar code</param>
        /// <param name="direction">CodeDirection - Indicates the direction to draw the bar code</param>
        /// <param name="code128Code">Code_128_Code_Types - Indicates which of the codes to use when rendering the bar code.
        /// The options are A, B, or buffer.</param>
        public Code128(byte[] values, string text, XSize size, CodeDirection direction, Code128CodeType code128Code)
        : base(text, size, direction)
        {
            if (!Enum.IsDefined(typeof(Code128CodeType), code128Code)) throw new ArgumentOutOfRangeException("Parameter code128Code (Code_128_Code_Types) is invalid");
            if (Patterns == null) Load();
            Code128Code = code128Code;
            //Values = values;
        }

        /// <summary>
        /// TODO: Validate Application Identifiers (AI)
        /// Parse text to raw data with start codes. Optimizes data if possible.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ParseTextToBarCode(string text)
        {
            Code128CodeType startCode = GetStartCode(text, Code128Code);
            Code128CodeType currentCode = startCode;

            string values = string.Empty;
            values += (char)startCode;

            int index = 0;
            while (index < text.Length)
            {
                if (Code128Code == Code128CodeType.Auto
                    && (currentCode == Code128CodeType.CodeA || currentCode == Code128CodeType.CodeB)
                    && GetNumberCount(text, index) >= 4)
                {
                    currentCode = Code128CodeType.CodeC;
                    values += CodeC;
                    continue;
                }

                int charNumber = text[index];
                if (charNumber == FNC1)
                {
                    values += FNC1;
                    index++;
                    continue;
                }

                if (currentCode == Code128CodeType.CodeA)
                {
                    if (charNumber < 32)
                        values += (char)(charNumber + 64);
                    else if (charNumber >= 95)
                    {
                        currentCode = Code128CodeType.CodeB;
                        values += CodeB;
                        values += (char)(charNumber - 32);
                    }
                    else
                        values += (char)(charNumber - 32);

                    index++;
                }
                else if (currentCode == Code128CodeType.CodeB)
                {
                    if (IsCharAType(charNumber))
                    {
                        currentCode = Code128CodeType.CodeA;
                        values += CodeA;
                        values += (char)(charNumber + 64);
                    }
                    else
                        values += (char)(charNumber - 32);

                    index++;
                }
                else if (currentCode == Code128CodeType.CodeC)
                {
                    if (GetNumberCount(text, index) >= 2)
                    {
                        var valuePair = GetNumberPairChar(text, index);
                        values += valuePair.Value;
                        index += valuePair.Key;
                    }
                    else
                    {
                        if (IsCharAType(charNumber))
                        {
                            currentCode = Code128CodeType.CodeA;
                            values += CodeA;
                            values += (char)(charNumber + 64);
                        }
                        else
                        {
                            currentCode = Code128CodeType.CodeB;
                            values += CodeB;
                            values += (char)(charNumber - 32);
                        }

                        index++;
                    }
                }

                if (Code128Code != Code128CodeType.Auto && currentCode != startCode)
                    throw new Exception($"Invalid characters in barcode for CodeType at index {index}");
            }

            return values.ToString();
        }

        private bool IsCharAType(int charNumber) => charNumber < 32;

        /// <summary>
        /// Add FNC1 and remove unwanted characters
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string PrepareGS1Text(string text)
        {
            Regex regex = new Regex("[^a-zA-Z0-9( -]");
            text = regex.Replace(text, "");
            text = text.Replace('(', FNC1);
            return text;
        }

        /// <summary>
        /// Get optimized char from value
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private KeyValuePair<int, string> GetNumberPairChar(string inputText, int startIndex)
        {
            var values = new StringBuilder();
            string text = inputText.Substring(startIndex);

            int index = 0;
            if (text.StartsWith(FNC1.ToString()))
            {
                values.Append(FNC1);
                index++;
            }

            //ASCII 0 = 48, 9 = 57
            int firstChar = text[index++] - 48;
            int secondChar = text[index++] - 48;

            char optimizedChar = (char)(firstChar * 10 + secondChar);
            values.Append(optimizedChar);

            return new KeyValuePair<int, string>(index, values.ToString());
        }

        /// <summary>
        /// Get start code
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Code128CodeType GetStartCode(string text, Code128CodeType type)
        {
            if (type == Code128CodeType.Auto && GetNumberCount(text, 0) >= 2)
                return Code128CodeType.CodeC;
            else if (type == Code128CodeType.Auto)
            {
                int firstValue = text[0];
                if (firstValue < 32)
                    return Code128CodeType.CodeA;
                else
                    return Code128CodeType.CodeB;
            }

            return type;
        }

        /// <summary>
        /// Get number count in text, ignores FNC1
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private int GetNumberCount(string text, int startIndex)
        {
            int count = 0;
            foreach (var character in text.Substring(startIndex))
            {
                if (character == FNC1)
                    continue;
                else if (char.IsNumber(character))
                    count++;
                else
                    break;
            }

            return count;
        }

        private void CheckValues()
        {
            if (BarCode == null) throw new InvalidOperationException("Text or Values must be set");
            if (BarCode.Length == 0) throw new InvalidOperationException("Text or Values must have content");

            for (int x = 0; x < BarCode.Length; x++)
            {
                var charNumber = BarCode[x];
                if (charNumber > 127 && charNumber != FNC1) throw new ArgumentOutOfRangeException(BcgSR.InvalidCode128(x));
            }
        }

        /// <summary>Creates a new instance of the Patterns field and populates it with the appropriate
        /// pattern to draw a code 128 bar code</summary>
        private void Load()
        {
            Patterns = new Dictionary<byte, byte[]>();
            Patterns.Add(0, new byte[] { 2, 1, 2, 2, 2, 2 });
            Patterns.Add(1, new byte[] { 2, 2, 2, 1, 2, 2 });
            Patterns.Add(2, new byte[] { 2, 2, 2, 2, 2, 1 });
            Patterns.Add(3, new byte[] { 1, 2, 1, 2, 2, 3 });
            Patterns.Add(4, new byte[] { 1, 2, 1, 3, 2, 2 });
            Patterns.Add(5, new byte[] { 1, 3, 1, 2, 2, 2 });
            Patterns.Add(6, new byte[] { 1, 2, 2, 2, 1, 3 });
            Patterns.Add(7, new byte[] { 1, 2, 2, 3, 1, 2 });
            Patterns.Add(8, new byte[] { 1, 3, 2, 2, 1, 2 });
            Patterns.Add(9, new byte[] { 2, 2, 1, 2, 1, 3 });
            Patterns.Add(10, new byte[] { 2, 2, 1, 3, 1, 2 });
            Patterns.Add(11, new byte[] { 2, 3, 1, 2, 1, 2 });
            Patterns.Add(12, new byte[] { 1, 1, 2, 2, 3, 2 });
            Patterns.Add(13, new byte[] { 1, 2, 2, 1, 3, 2 });
            Patterns.Add(14, new byte[] { 1, 2, 2, 2, 3, 1 });
            Patterns.Add(15, new byte[] { 1, 1, 3, 2, 2, 2 });
            Patterns.Add(16, new byte[] { 1, 2, 3, 1, 2, 2 });
            Patterns.Add(17, new byte[] { 1, 2, 3, 2, 2, 1 });
            Patterns.Add(18, new byte[] { 2, 2, 3, 2, 1, 1 });
            Patterns.Add(19, new byte[] { 2, 2, 1, 1, 3, 2 });
            Patterns.Add(20, new byte[] { 2, 2, 1, 2, 3, 1 });
            Patterns.Add(21, new byte[] { 2, 1, 3, 2, 1, 2 });
            Patterns.Add(22, new byte[] { 2, 2, 3, 1, 1, 2 });
            Patterns.Add(23, new byte[] { 3, 1, 2, 1, 3, 1 });
            Patterns.Add(24, new byte[] { 3, 1, 1, 2, 2, 2 });
            Patterns.Add(25, new byte[] { 3, 2, 1, 1, 2, 2 });
            Patterns.Add(26, new byte[] { 3, 2, 1, 2, 2, 1 });
            Patterns.Add(27, new byte[] { 3, 1, 2, 2, 1, 2 });
            Patterns.Add(28, new byte[] { 3, 2, 2, 1, 1, 2 });
            Patterns.Add(29, new byte[] { 3, 2, 2, 2, 1, 1 });
            Patterns.Add(30, new byte[] { 2, 1, 2, 1, 2, 3 });
            Patterns.Add(31, new byte[] { 2, 1, 2, 3, 2, 1 });
            Patterns.Add(32, new byte[] { 2, 3, 2, 1, 2, 1 });
            Patterns.Add(33, new byte[] { 1, 1, 1, 3, 2, 3 });
            Patterns.Add(34, new byte[] { 1, 3, 1, 1, 2, 3 });
            Patterns.Add(35, new byte[] { 1, 3, 1, 3, 2, 1 });
            Patterns.Add(36, new byte[] { 1, 1, 2, 3, 1, 3 });
            Patterns.Add(37, new byte[] { 1, 3, 2, 1, 1, 3 });
            Patterns.Add(38, new byte[] { 1, 3, 2, 3, 1, 1 });
            Patterns.Add(39, new byte[] { 2, 1, 1, 3, 1, 3 });
            Patterns.Add(40, new byte[] { 2, 3, 1, 1, 1, 3 });
            Patterns.Add(41, new byte[] { 2, 3, 1, 3, 1, 1 });
            Patterns.Add(42, new byte[] { 1, 1, 2, 1, 3, 3 });
            Patterns.Add(43, new byte[] { 1, 1, 2, 3, 3, 1 });
            Patterns.Add(44, new byte[] { 1, 3, 2, 1, 3, 1 });
            Patterns.Add(45, new byte[] { 1, 1, 3, 1, 2, 3 });
            Patterns.Add(46, new byte[] { 1, 1, 3, 3, 2, 1 });
            Patterns.Add(47, new byte[] { 1, 3, 3, 1, 2, 1 });
            Patterns.Add(48, new byte[] { 3, 1, 3, 1, 2, 1 });
            Patterns.Add(49, new byte[] { 2, 1, 1, 3, 3, 1 });
            Patterns.Add(50, new byte[] { 2, 3, 1, 1, 3, 1 });
            Patterns.Add(51, new byte[] { 2, 1, 3, 1, 1, 3 });
            Patterns.Add(52, new byte[] { 2, 1, 3, 3, 1, 1 });
            Patterns.Add(53, new byte[] { 2, 1, 3, 1, 3, 1 });
            Patterns.Add(54, new byte[] { 3, 1, 1, 1, 2, 3 });
            Patterns.Add(55, new byte[] { 3, 1, 1, 3, 2, 1 });
            Patterns.Add(56, new byte[] { 3, 3, 1, 1, 2, 1 });
            Patterns.Add(57, new byte[] { 3, 1, 2, 1, 1, 3 });
            Patterns.Add(58, new byte[] { 3, 1, 2, 3, 1, 1 });
            Patterns.Add(59, new byte[] { 3, 3, 2, 1, 1, 1 });
            Patterns.Add(60, new byte[] { 3, 1, 4, 1, 1, 1 });
            Patterns.Add(61, new byte[] { 2, 2, 1, 4, 1, 1 });
            Patterns.Add(62, new byte[] { 4, 3, 1, 1, 1, 1 });
            Patterns.Add(63, new byte[] { 1, 1, 1, 2, 2, 4 });
            Patterns.Add(64, new byte[] { 1, 1, 1, 4, 2, 2 });
            Patterns.Add(65, new byte[] { 1, 2, 1, 1, 2, 4 });
            Patterns.Add(66, new byte[] { 1, 2, 1, 4, 2, 1 });
            Patterns.Add(67, new byte[] { 1, 4, 1, 1, 2, 2 });
            Patterns.Add(68, new byte[] { 1, 4, 1, 2, 2, 1 });
            Patterns.Add(69, new byte[] { 1, 1, 2, 2, 1, 4 });
            Patterns.Add(70, new byte[] { 1, 1, 2, 4, 1, 2 });
            Patterns.Add(71, new byte[] { 1, 2, 2, 1, 1, 4 });
            Patterns.Add(72, new byte[] { 1, 2, 2, 4, 1, 1 });
            Patterns.Add(73, new byte[] { 1, 4, 2, 1, 1, 2 });
            Patterns.Add(74, new byte[] { 1, 4, 2, 2, 1, 1 });
            Patterns.Add(75, new byte[] { 2, 4, 1, 2, 1, 1 });
            Patterns.Add(76, new byte[] { 2, 2, 1, 1, 1, 4 });
            Patterns.Add(77, new byte[] { 4, 1, 3, 1, 1, 1 });
            Patterns.Add(78, new byte[] { 2, 4, 1, 1, 1, 2 });
            Patterns.Add(79, new byte[] { 1, 3, 4, 1, 1, 1 });
            Patterns.Add(80, new byte[] { 1, 1, 1, 2, 4, 2 });
            Patterns.Add(81, new byte[] { 1, 2, 1, 1, 4, 2 });
            Patterns.Add(82, new byte[] { 1, 2, 1, 2, 4, 1 });
            Patterns.Add(83, new byte[] { 1, 1, 4, 2, 1, 2 });
            Patterns.Add(84, new byte[] { 1, 2, 4, 1, 1, 2 });
            Patterns.Add(85, new byte[] { 1, 2, 4, 2, 1, 1 });
            Patterns.Add(86, new byte[] { 4, 1, 1, 2, 1, 2 });
            Patterns.Add(87, new byte[] { 4, 2, 1, 1, 1, 2 });
            Patterns.Add(88, new byte[] { 4, 2, 1, 2, 1, 1 });
            Patterns.Add(89, new byte[] { 2, 1, 2, 1, 4, 1 });
            Patterns.Add(90, new byte[] { 2, 1, 4, 1, 2, 1 });
            Patterns.Add(91, new byte[] { 4, 1, 2, 1, 2, 1 });
            Patterns.Add(92, new byte[] { 1, 1, 1, 1, 4, 3 });
            Patterns.Add(93, new byte[] { 1, 1, 1, 3, 4, 1 });
            Patterns.Add(94, new byte[] { 1, 3, 1, 1, 4, 1 });
            Patterns.Add(95, new byte[] { 1, 1, 4, 1, 1, 3 });
            Patterns.Add(96, new byte[] { 1, 1, 4, 3, 1, 1 });
            Patterns.Add(97, new byte[] { 4, 1, 1, 1, 1, 3 });
            Patterns.Add(98, new byte[] { 4, 1, 1, 3, 1, 1 });
            Patterns.Add(99, new byte[] { 1, 1, 3, 1, 4, 1 });
            Patterns.Add(100, new byte[] { 1, 1, 4, 1, 3, 1 });
            Patterns.Add(101, new byte[] { 3, 1, 1, 1, 4, 1 });
            Patterns.Add(102, new byte[] { 4, 1, 1, 1, 3, 1 });
            Patterns.Add(103, new byte[] { 2, 1, 1, 4, 1, 2 });
            Patterns.Add(104, new byte[] { 2, 1, 1, 2, 1, 4 });
            Patterns.Add(105, new byte[] { 2, 1, 1, 2, 3, 2 });
            Patterns.Add(106, new byte[] { 2, 3, 3, 1, 1, 1, 2 });
        }

        /// <summary>Validates the text string to be coded</summary>
        /// <param name="text">String - The text string to be coded</param>
        protected override void CheckCode(string text)
        {
            if (text == null) throw new ArgumentNullException("Parameter text (string) can not be null");
            if (text.Length == 0) throw new ArgumentException("Parameter text (string) can not be empty");
        }

        /// <summary>Renders the content found in Text</summary>
        /// <param name="gfx">XGraphics - Instance of the drawing surface </param>
        /// <param name="brush">XBrush - Line and Color to draw the bar code</param>
        /// <param name="font">XFont - Font to use to draw the text string</param>
        /// <param name="position">XPoint - Location to render the bar code</param>
        protected internal override void Render(XGraphics gfx, XBrush brush, XFont font, XPoint position)
        {
            XGraphicsState state = gfx.Save();

            BarCodeRenderInfo info = new BarCodeRenderInfo(gfx, brush, font, position);
            InitRendering(info);
            info.CurrPosInString = 0;
            info.CurrPos = position - CodeBase.CalcDistance(AnchorType.TopLeft, this.Anchor, this.Size);

            foreach (char c in BarCode)
                RenderValue(info, c);

            RenderStop(info);
            if (TextLocation != TextLocation.None) RenderText(info);

            gfx.Restore(state);
        }

        private void RenderStop(BarCodeRenderInfo info)
        {
            RenderValue(info, CalculateParity());
            RenderValue(info, STOPCODE);
        }

        private void RenderValue(BarCodeRenderInfo info, int chVal)
        {
            byte[] pattern = GetPattern(chVal);
            XBrush space = XBrushes.White;
            for (int idx = 0; idx < pattern.Length; idx++)
            {
                if ((idx % 2) == 0)
                {
                    RenderBar(info, info.ThinBarWidth * pattern[idx]);
                }
                else
                {
                    RenderBar(info, info.ThinBarWidth * pattern[idx], space);
                }
            }
        }

        private void RenderText(BarCodeRenderInfo info)
        {
            if (info.Font == null) info.Font = new XFont("Courier New", Size.Height / 6);
            XPoint center = info.Position + CodeBase.CalcDistance(this.Anchor, AnchorType.TopLeft, this.Size);
            if (TextLocation == TextLocation.Above)
            {
                info.Gfx.DrawString(this.Text, info.Font, info.Brush, new XRect(center, Size), XStringFormats.TopCenter);
            }
            else if (TextLocation == TextLocation.AboveEmbedded)
            {
                XSize textSize = info.Gfx.MeasureString(this.Text, info.Font);
                textSize.Width += this.Size.Width * .15;
                XPoint point = info.Position;
                point.X += (this.Size.Width - textSize.Width) / 2;
                XRect rect = new XRect(point, textSize);
                info.Gfx.DrawRectangle(XBrushes.White, rect);
                info.Gfx.DrawString(this.Text, info.Font, info.Brush, new XRect(center, Size), XStringFormats.TopCenter);
            }
            else if (TextLocation == TextLocation.Below)
            {
                info.Gfx.DrawString(this.Text, info.Font, info.Brush, new XRect(center, Size), XStringFormats.BottomCenter);
            }
            else if (TextLocation == TextLocation.BelowEmbedded)
            {
                XSize textSize = info.Gfx.MeasureString(this.Text, info.Font);
                textSize.Width += this.Size.Width * .15;
                XPoint point = info.Position;
                point.X += (this.Size.Width - textSize.Width) / 2;
                point.Y += Size.Height - textSize.Height;
                XRect rect = new XRect(point, textSize);
                info.Gfx.DrawRectangle(XBrushes.White, rect);
                info.Gfx.DrawString(this.Text, info.Font, info.Brush, new XRect(center, Size), XStringFormats.BottomCenter);
            }
        }

        private byte[] GetPattern(int codeValue)
        {
            if (codeValue < 0) throw new ArgumentOutOfRangeException("Parameter ch (int) can not be less than 32 (space).");
            if (codeValue > 106) throw new ArgumentOutOfRangeException("Parameter ch (int) can not be greater than 138.");
            return Patterns[(byte)codeValue];
        }

        private int CalculateParity()
        {
            long parityValue = BarCode[0];
            for (int x = 1; x < BarCode.Length; x++)
            {
                parityValue += (BarCode[x] * x);
            }
            parityValue %= 103;
            return (int)parityValue;
        }

        /// <summary>Renders a single line of the character. Each character has three lines and three spaces</summary>
        /// <param name="info"></param>
        /// <param name="barWidth">Indicates the thickness of the line/bar to be rendered.</param>
        internal void RenderBar(BarCodeRenderInfo info, double barWidth)
        {
            RenderBar(info, barWidth, info.Brush);
        }

        /// <summary>Renders a single line of the character. Each character has three lines and three spaces</summary>
        /// <param name="info"></param>
        /// <param name="barWidth">Indicates the thickness of the line/bar to be rendered.</param>
        /// <param name="brush">Indicates the brush to use to render the line/bar.</param>
        private void RenderBar(BarCodeRenderInfo info, double barWidth, XBrush brush)
        {
            double height = Size.Height;
            double yPos = info.CurrPos.Y;

            switch (TextLocation)
            {
                case TextLocation.Above:
                    yPos = info.CurrPos.Y + (height / 5);
                    height *= 4.0 / 5;
                    break;
                case TextLocation.Below:
                    height *= 4.0 / 5;
                    break;
                case TextLocation.AboveEmbedded:
                case TextLocation.BelowEmbedded:
                case TextLocation.None:
                    break;
            }
            XRect rect = new XRect(info.CurrPos.X, yPos, barWidth, height);
            info.Gfx.DrawRectangle(brush, rect);
            info.CurrPos.X += barWidth;
        }

        internal override void InitRendering(BarCodeRenderInfo info)
        {
            if (BarCode == null) throw new InvalidOperationException(BcgSR.BarCodeNotSet);
            if (BarCode.Length == 0) throw new InvalidOperationException(BcgSR.EmptyBarCodeSize);

            int numberOfBars = BarCode.Length + 2; // The length of the string stop, and parity value
            numberOfBars *= 11; // Each character has 11 bars
            numberOfBars += 2; // Add two more because the stop bit has two extra bars

            // Calculating the width of a bar
            info.ThinBarWidth = ((double)this.Size.Width / (double)numberOfBars);
        }
    }


    /// <summary>Code types for Code 128 bar code</summary>
    public enum Code128CodeType
    {
        /// <summary>Optimize code based on content</summary>
        Auto = 0,
        /// <summary>Code A</summary>
        CodeA = 103,
        /// <summary>Code B</summary>
        CodeB = 104,
        /// <summary>Code buffer</summary>
        CodeC = 105,
    }
}