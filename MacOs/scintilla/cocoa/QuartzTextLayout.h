/*
 *  QuartzTextLayout.h
 */

#ifndef _QUARTZ_TEXT_LAYOUT_H
#define _QUARTZ_TEXT_LAYOUT_H

#include <Cocoa/Cocoa.h>
#include <CoreText/CoreText.h>

#include "QuartzTextStyle.h"

class QuartzTextLayout
{
public:
    explicit QuartzTextLayout(CGContextRef context) : text(NULL), attributedText(NULL), line(NULL), gc(context)
    {
    }

    ~QuartzTextLayout()
    {
        if (line != NULL)
            CFRelease(line);
        if (attributedText != NULL)
            CFRelease(attributedText);
        if (text != NULL)
            CFRelease(text);
    }

    OSStatus setText(const UInt8 *buffer, size_t byteLength, CFStringEncoding encoding)
    {
        clearLineObjects();

        text = CFStringCreateWithBytes(kCFAllocatorDefault, buffer, byteLength, encoding, false);
        if (text == NULL)
            return -1;

        attributedText = CFAttributedStringCreate(kCFAllocatorDefault, text, NULL);
        if (attributedText == NULL)
            return -1;

        line = CTLineCreateWithAttributedString(attributedText);
        return (line != NULL) ? noErr : -1;
    }

    void setText(const UInt8 *buffer, size_t byteLength, const QuartzTextStyle &style)
    {
        if (setText(buffer, byteLength, kCFStringEncodingUTF8) == noErr)
            setStyle(style);
    }

    void setStyle(const QuartzTextStyle &style)
    {
        if (text == NULL)
            return;

        if (line != NULL)
        {
            CFRelease(line);
            line = NULL;
        }
        if (attributedText != NULL)
        {
            CFRelease(attributedText);
            attributedText = NULL;
        }

        CFMutableDictionaryRef attributes = CFDictionaryCreateMutable(kCFAllocatorDefault,
                                                                      0,
                                                                      &kCFTypeDictionaryKeyCallBacks,
                                                                      &kCFTypeDictionaryValueCallBacks);
        if (attributes == NULL)
            return;

        CTFontRef font = style.copyCTFont();
        if (font != NULL)
        {
            CFDictionarySetValue(attributes, kCTFontAttributeName, font);
            CFRelease(font);
        }

        if (style.isUnderlineEnabled())
        {
            int32_t underlineStyle = kCTUnderlineStyleSingle;
            CFNumberRef underline = CFNumberCreate(kCFAllocatorDefault, kCFNumberSInt32Type, &underlineStyle);
            if (underline != NULL)
            {
                CFDictionarySetValue(attributes, kCTUnderlineStyleAttributeName, underline);
                CFRelease(underline);
            }
        }

        attributedText = CFAttributedStringCreate(kCFAllocatorDefault, text, attributes);
        CFRelease(attributes);
        if (attributedText != NULL)
            line = CTLineCreateWithAttributedString(attributedText);
    }

    void draw(float x, float y, bool flipTextYAxis = false)
    {
        if (line == NULL || gc == NULL)
            return;

        if (flipTextYAxis)
        {
            CGContextSaveGState(gc);
            CGContextScaleCTM(gc, 1.0, -1.0);
            y = -y;
        }

        CGContextSetTextPosition(gc, x, y);
        CTLineDraw(line, gc);

        if (flipTextYAxis)
            CGContextRestoreGState(gc);
    }

    int width() const
    {
        if (line == NULL)
            return 0;
        double ascent = 0;
        double descent = 0;
        double leading = 0;
        double w = CTLineGetTypographicBounds(line, &ascent, &descent, &leading);
        return static_cast<int>(w + 0.5);
    }

    inline CFIndex getLength() const
    {
        return (text != NULL) ? CFStringGetLength(text) : 0;
    }

    inline void setContext(CGContextRef context)
    {
        gc = context;
    }

private:
    void clearLineObjects()
    {
        if (line != NULL)
        {
            CFRelease(line);
            line = NULL;
        }
        if (attributedText != NULL)
        {
            CFRelease(attributedText);
            attributedText = NULL;
        }
        if (text != NULL)
        {
            CFRelease(text);
            text = NULL;
        }
    }

    CFStringRef text;
    CFAttributedStringRef attributedText;
    CTLineRef line;
    CGContextRef gc;
};

#endif
