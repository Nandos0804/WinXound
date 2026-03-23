/* -*- Mode: C; indent-tabs-mode: t; c-basic-offset: 4; tab-width: 4 -*- */
/*
 * gtkmm3 compatibility wrapper for gtkmm2 Menu_Helpers API
 */

#ifndef _GTKMM_COMPAT_H_
#define _GTKMM_COMPAT_H_

#include <gtkmm.h>

#ifdef WINXOUND_MODERN_DEPS

// Compatibility namespace for gtkmm2 Menu_Helpers in gtkmm3
namespace Gtk {
namespace Menu_Helpers {

// Helper to create and store menu items with signals
// This creates a temporary that will be moved into the menu list
class MenuElemHelper {
  public:
    template <typename SlotType>
    static Gtk::MenuItem create_with_signal(const Glib::ustring &label, const SlotType &slot) {
        Gtk::MenuItem item(label);
        item.signal_activate().connect(slot);
        return item;
    }
};

// Store menu item signals in a static map since gtkmm3 doesn't auto-preserve them
template <typename SlotType>
inline Gtk::MenuItem MenuElem(const Glib::ustring &label, const SlotType &slot) {
    Gtk::MenuItem item(label);
    item.signal_activate().connect(slot);
    return item;
}

// SeparatorElem replacement for gtkmm3
inline Gtk::SeparatorMenuItem SeparatorElem() { return Gtk::SeparatorMenuItem(); }

} // namespace Menu_Helpers
} // namespace Gtk

#endif // WINXOUND_MODERN_DEPS

#endif // _GTKMM_COMPAT_H_
