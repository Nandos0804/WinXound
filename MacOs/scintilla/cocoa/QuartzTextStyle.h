/*
 *  QuartzTextStyle.h
 */

#ifndef _QUARTZ_TEXT_STYLE_H
#define _QUARTZ_TEXT_STYLE_H

#include "QuartzTextStyleAttribute.h"

#include <CoreText/CoreText.h>

#include <cmath>

class QuartzTextStyle
{
public:
    QuartzTextStyle() : fontName("Menlo"), pointSize(12.0), bold(false), italic(false), underline(false), fontCache(NULL)
    {
    }

    ~QuartzTextStyle()
    {
        if (fontCache != NULL)
            CFRelease(fontCache);
        fontCache = NULL;
    }

    void setAttributes(QuartzTextStyleAttribute *attributes[], int number)
    {
        for (int i = 0; i < number; ++i)
        {
            if (QuartzFont *font = dynamic_cast<QuartzFont *>(attributes[i]))
            {
                fontName = font->getName();
            }
            else if (QuartzTextSize *size = dynamic_cast<QuartzTextSize *>(attributes[i]))
            {
                pointSize = size->getPointSize();
            }
            else if (QuartzTextBold *isBold = dynamic_cast<QuartzTextBold *>(attributes[i]))
            {
                bold = isBold->getValue();
            }
            else if (QuartzTextItalic *isItalic = dynamic_cast<QuartzTextItalic *>(attributes[i]))
            {
                italic = isItalic->getValue();
            }
            else if (QuartzTextUnderline *isUnderline = dynamic_cast<QuartzTextUnderline *>(attributes[i]))
            {
                underline = isUnderline->getValue();
            }
        }
        invalidateFontCache();
    }

    void setFontFeature(int /*featureType*/, int /*selector*/)
    {
        // No-op on modern Cocoa path.
    }

    CTFontRef copyCTFont() const
    {
        CTFontRef font = ensureCTFont();
        if (font == NULL)
            return NULL;
        CFRetain(font);
        return font;
    }

    const std::string &getFontName() const
    {
        return fontName;
    }

    CGFloat getPointSize() const
    {
        return pointSize;
    }

    bool isUnderlineEnabled() const
    {
        return underline;
    }

    int ascentPixels() const
    {
        CTFontRef font = ensureCTFont();
        return (font != NULL) ? static_cast<int>(CTFontGetAscent(font) + 0.5) : 0;
    }

    int descentPixels() const
    {
        CTFontRef font = ensureCTFont();
        return (font != NULL) ? static_cast<int>(CTFontGetDescent(font) + 0.5) : 0;
    }

    int leadingPixels() const
    {
        CTFontRef font = ensureCTFont();
        return (font != NULL) ? static_cast<int>(CTFontGetLeading(font) + 0.5) : 0;
    }

private:
    CTFontRef ensureCTFont() const
    {
        if (fontCache == NULL)
        {
            CFStringRef fontNameRef = CFStringCreateWithCString(kCFAllocatorDefault, fontName.c_str(), kCFStringEncodingUTF8);
            if (fontNameRef == NULL)
            {
                fontNameRef = CFSTR("Menlo");
                CFRetain(fontNameRef);
            }

            CTFontRef baseFont = CTFontCreateWithName(fontNameRef, pointSize, NULL);
            CFRelease(fontNameRef);

            if (baseFont == NULL)
                baseFont = CTFontCreateWithName(CFSTR("Menlo"), pointSize, NULL);
            if (baseFont == NULL)
                baseFont = CTFontCreateUIFontForLanguage(kCTFontUserFixedPitchFontType, pointSize, NULL);
            if (baseFont == NULL)
                baseFont = CTFontCreateUIFontForLanguage(kCTFontSystemFontType, pointSize, NULL);

            CTFontSymbolicTraits traits = 0;
            if (bold)
                traits |= kCTFontBoldTrait;
            if (italic)
                traits |= kCTFontItalicTrait;

            if (baseFont != NULL && traits != 0)
            {
                CTFontRef converted = CTFontCreateCopyWithSymbolicTraits(baseFont, pointSize, NULL, traits, traits);
                if (converted != NULL)
                {
                    CFRelease(baseFont);
                    baseFont = converted;
                }
            }

            fontCache = baseFont;
        }

        return fontCache;
    }

    void invalidateFontCache()
    {
        if (fontCache != NULL)
            CFRelease(fontCache);
        fontCache = NULL;
    }

    std::string fontName;
    CGFloat pointSize;
    bool bold;
    bool italic;
    bool underline;
    mutable CTFontRef fontCache;
};

#endif

