
This is a non-functional library. 

It's purpose is to isolate 
```csharp
public OpenTypeFontface CreateFontSubSet(Dictionary<int, object> glyphs, bool cidFont)
```
found in 
```
OpenType\Fonts.OpenType\OpenTypeFontface.cs
```

Pdf.Advanced\PdfCIDFont.cs => PrepareForSave
```csharp
// CID fonts must be always embedded. PDFsharp embeds automatically a subset.
OpenTypeFontface subSet = null;
if (FontDescriptor._descriptor.FontFace.loca == null)
    subSet = FontDescriptor._descriptor.FontFace;
else
    subSet = FontDescriptor._descriptor.FontFace.CreateFontSubSet(_cmapInfo.GlyphIndices, true);
byte[] fontData = subSet.FontSource.Bytes;

internal CMapInfo _cmapInfo;
```

Pdf.Advanced\PdfFont.cs
```csharp
internal void AddChars(string text)
{
	if (_cmapInfo != null)
		_cmapInfo.AddChars(text);
}

/// <summary>
/// Adds the characters of the specified string to the hashtable.
/// </summary>
public void AddChars(string text)
{
    if (text != null)
    {
        bool symbol = _descriptor.FontFace.cmap.symbol;
        int length = text.Length;
        for (int idx = 0; idx < length; idx++)
        {
            char ch = text[idx];
            if (!CharacterToGlyphIndex.ContainsKey(ch))
            {
                char ch2 = ch;
                if (symbol)
                {
                    // Remap ch for symbol fonts.
                    ch2 = (char)(ch | (_descriptor.FontFace.os2.usFirstCharIndex & 0xFF00));  // @@@ refactor
                }
                int glyphIndex = _descriptor.CharCodeToGlyphIndex(ch2);
                CharacterToGlyphIndex.Add(ch, glyphIndex);
                GlyphIndices[glyphIndex] = null;
                MinChar = (char)Math.Min(MinChar, ch);
                MaxChar = (char)Math.Max(MaxChar, ch);
            }
        }
    }
}
```

https://blog.idrsolutions.com/2011/03/understanding-the-pdf-file-format-%E2%80%93-what-are-cid-fonts/

Understanding the PDF file Format ?What are CID fonts



There are 2 main font technologies used in PDF font files (Postscript/Type1 and Truetype). 
There is also a �merged?format which borrows features from both (OpenType). 


Both however are very good for displaying European style text 
(ie French, Engligh, German) with limited numbers of characters. 
They are less suited to languages such as Chinese or Japanese. 
This is where CID fonts come in ?they are extensions 
of these font technologies to provide better support for these languages. 
CidFontType0 extends Type1 (Postscript) while CidFontType2 extends TrueType.

CID fonts are also better at allowing for text which does not have a left to right flow. 
There is even a vertical writing mode.

Adding these features onto the technically tried and test Type1/Truetype 
font technologies offers a very elegant way to display Chinese and Japanese glyfs.  
Which is another reason for the PDF file format�s popularity.



https://blog.idrsolutions.com/2013/01/understanding-the-pdf-file-format-overview/


https://fontforge.github.io/cidmenu.html

Er... What is a CID keyed Font?

A CID keyed font is a postscript (or opentype) font designed to hold 
Chinese, Japanese and Korean characters efficiently. 
More accurately a CID font is a collection of several sub-fonts 
each with certain common features 
(one might hold all the latin letters, another all the kana, a third all the kanji). 
This allows font-wide hinting to be crafted for subsets of glyphs 
to which have something in common.

CID keyed fonts do not have an encoding built into the font, 
and the glyphs do not have names. 
Instead the font is associated with a glyph set and on each glyph set 
there are several character mappings defined. 
These mappings are similar to encodings 
but allow for a wider range of behaviors.

A CID is a glyph index and is used to look up glyph descriptions 
instead of glyph names in other types of fonts. 
Using a glyph set FontForge will often be able to map a CID to a unicode character name 
(but not always), so FontForge will give glyphs names when it can.

For more information see the section on CID keyed fonts on the font view page.
