/*
 *  QuartzTextStyleAttribute.h
 */

#ifndef _QUARTZ_TEXT_STYLE_ATTRIBUTE_H
#define _QUARTZ_TEXT_STYLE_ATTRIBUTE_H

#include <Cocoa/Cocoa.h>

#include <cassert>
#include <string>

class QuartzTextStyleAttribute
{
public:
    QuartzTextStyleAttribute() {}
    virtual ~QuartzTextStyleAttribute() {}
};

class QuartzTextSize : public QuartzTextStyleAttribute
{
public:
    explicit QuartzTextSize(float points) : pointSize(points) {}
    CGFloat getPointSize() const { return pointSize; }

private:
    CGFloat pointSize;
};

class QuartzTextStyleAttributeBoolean : public QuartzTextStyleAttribute
{
public:
    explicit QuartzTextStyleAttributeBoolean(bool newVal) : value(newVal) {}
    bool getValue() const { return value; }

private:
    bool value;
};

class QuartzTextBold : public QuartzTextStyleAttributeBoolean
{
public:
    explicit QuartzTextBold(bool newVal) : QuartzTextStyleAttributeBoolean(newVal) {}
};

class QuartzTextItalic : public QuartzTextStyleAttributeBoolean
{
public:
    explicit QuartzTextItalic(bool newVal) : QuartzTextStyleAttributeBoolean(newVal) {}
};

class QuartzTextUnderline : public QuartzTextStyleAttributeBoolean
{
public:
    explicit QuartzTextUnderline(bool newVal) : QuartzTextStyleAttributeBoolean(newVal) {}
};

class QuartzFont : public QuartzTextStyleAttribute
{
public:
    QuartzFont(const char *name, int length)
    {
        assert(name != NULL && length > 0 && name[length] == '\0');
        fontName.assign(name, static_cast<size_t>(length));
    }

    const std::string &getName() const
    {
        return fontName;
    }

private:
    std::string fontName;
};

#endif

