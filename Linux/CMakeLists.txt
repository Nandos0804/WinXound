cmake_minimum_required(VERSION 3.16)

project(WinXoundLinux LANGUAGES C CXX)

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

find_package(PkgConfig REQUIRED)

set(WINXOUND_DEP_PROFILE "auto" CACHE STRING "Dependency profile: auto, modern, or legacy")
set_property(CACHE WINXOUND_DEP_PROFILE PROPERTY STRINGS auto modern legacy)

set(WINXOUND_SELECTED_PROFILE "")

if(WINXOUND_DEP_PROFILE STREQUAL "modern" OR WINXOUND_DEP_PROFILE STREQUAL "auto")
    pkg_check_modules(GTKMM_MODERN QUIET gtkmm-3.0>=3.22)
    pkg_check_modules(VTE_MODERN QUIET vte-2.91)
    pkg_check_modules(WEBKIT_MODERN QUIET webkit2gtk-4.1)
    if(NOT WEBKIT_MODERN_FOUND)
        pkg_check_modules(WEBKIT_MODERN QUIET webkit2gtk-4.0)
    endif()

    if(GTKMM_MODERN_FOUND AND VTE_MODERN_FOUND AND WEBKIT_MODERN_FOUND)
        set(WINXOUND_SELECTED_PROFILE "modern")
        set(GTKMM_PREFIX GTKMM_MODERN)
        set(VTE_PREFIX VTE_MODERN)
        set(WEBKIT_PREFIX WEBKIT_MODERN)
    endif()
endif()

if(NOT WINXOUND_SELECTED_PROFILE AND (WINXOUND_DEP_PROFILE STREQUAL "legacy" OR WINXOUND_DEP_PROFILE STREQUAL "auto"))
    pkg_check_modules(GTKMM_LEGACY QUIET gtkmm-2.4>=2.12)
    pkg_check_modules(VTE_LEGACY QUIET vte)
    pkg_check_modules(WEBKIT_LEGACY QUIET webkit-1.0)

    if(GTKMM_LEGACY_FOUND AND VTE_LEGACY_FOUND AND WEBKIT_LEGACY_FOUND)
        set(WINXOUND_SELECTED_PROFILE "legacy")
        set(GTKMM_PREFIX GTKMM_LEGACY)
        set(VTE_PREFIX VTE_LEGACY)
        set(WEBKIT_PREFIX WEBKIT_LEGACY)
    endif()
endif()

if(NOT WINXOUND_SELECTED_PROFILE)
    message(FATAL_ERROR
        "No supported dependency profile found.\n"
        "Try installing one of these sets:\n"
        "  Modern: gtkmm-3.0, vte-2.91, webkit2gtk-4.1 (or 4.0)\n"
        "  Legacy: gtkmm-2.4, vte, webkit-1.0\n"
        "Or set -DWINXOUND_DEP_PROFILE=modern|legacy to force one profile."
    )
endif()

set(GTKMM_INCLUDE_DIRS ${${GTKMM_PREFIX}_INCLUDE_DIRS})
set(GTKMM_LIBRARIES ${${GTKMM_PREFIX}_LIBRARIES})
set(GTKMM_CFLAGS_OTHER ${${GTKMM_PREFIX}_CFLAGS_OTHER})

set(VTE_INCLUDE_DIRS ${${VTE_PREFIX}_INCLUDE_DIRS})
set(VTE_LIBRARIES ${${VTE_PREFIX}_LIBRARIES})
set(VTE_CFLAGS_OTHER ${${VTE_PREFIX}_CFLAGS_OTHER})

set(WEBKIT_INCLUDE_DIRS ${${WEBKIT_PREFIX}_INCLUDE_DIRS})
set(WEBKIT_LIBRARIES ${${WEBKIT_PREFIX}_LIBRARIES})
set(WEBKIT_CFLAGS_OTHER ${${WEBKIT_PREFIX}_CFLAGS_OTHER})

if(WINXOUND_SELECTED_PROFILE STREQUAL "legacy")
    pkg_check_modules(GTK_FOR_SCINTILLA REQUIRED gtk+-2.0)
else()
    pkg_check_modules(GTK_FOR_SCINTILLA REQUIRED gtk+-3.0)
endif()

message(STATUS "WinXound dependency profile: ${WINXOUND_SELECTED_PROFILE}")

set(SCINTILLA_CXX_SOURCES
    scintilla/gtk/PlatGTK.cxx
    scintilla/gtk/ScintillaGTK.cxx
)

set(SCINTILLA_C_SOURCES
    scintilla/gtk/scintilla-marshal.c
)

file(GLOB SCINTILLA_CORE_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/scintilla/src/*.cxx"
)

add_library(scintilla STATIC
    ${SCINTILLA_CXX_SOURCES}
    ${SCINTILLA_C_SOURCES}
    ${SCINTILLA_CORE_SOURCES}
)

target_include_directories(scintilla
    PRIVATE
        ${GTK_FOR_SCINTILLA_INCLUDE_DIRS}
        ${CMAKE_CURRENT_SOURCE_DIR}/scintilla/include
        ${CMAKE_CURRENT_SOURCE_DIR}/scintilla/src
)

target_compile_options(scintilla
    PRIVATE
        -Wall
        -Wno-missing-braces
        -Wno-char-subscripts
        -Wno-unused
        -Os
        ${GTK_FOR_SCINTILLA_CFLAGS_OTHER}
)

target_compile_definitions(scintilla PRIVATE GTK SCI_LEXER NDEBUG)
target_link_libraries(scintilla PRIVATE ${GTK_FOR_SCINTILLA_LIBRARIES})

file(GLOB WINXOUND_APP_SOURCES CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/src/*.cc"
)

add_executable(winxound ${WINXOUND_APP_SOURCES})

target_include_directories(winxound
    PRIVATE
        ${GTKMM_INCLUDE_DIRS}
        ${VTE_INCLUDE_DIRS}
        ${WEBKIT_INCLUDE_DIRS}
        ${CMAKE_CURRENT_SOURCE_DIR}/src
        ${CMAKE_CURRENT_SOURCE_DIR}/scintilla
)

target_compile_options(winxound
    PRIVATE
        -W
        -Wall
        -Wno-char-subscripts
        -Wno-unused
        -Os
        ${GTKMM_CFLAGS_OTHER}
        ${VTE_CFLAGS_OTHER}
        ${WEBKIT_CFLAGS_OTHER}
)

target_compile_definitions(winxound PRIVATE GTK SCI_LEXER)
if(WINXOUND_SELECTED_PROFILE STREQUAL "modern")
    target_compile_definitions(winxound PRIVATE WINXOUND_MODERN_DEPS=1)
else()
    target_compile_definitions(winxound PRIVATE WINXOUND_LEGACY_DEPS=1)
endif()

target_link_libraries(winxound
    PRIVATE
        scintilla
        ${GTKMM_LIBRARIES}
        ${VTE_LIBRARIES}
        ${WEBKIT_LIBRARIES}
)

set(STANDALONE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/WinXound")
set(STANDALONE_SRC_DIR "${STANDALONE_DIR}/src")

file(GLOB WINXOUND_UI_FILES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/*.ui")
file(GLOB WINXOUND_PNG_FILES CONFIGURE_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/src/*.png")

add_custom_target(standalone
    COMMAND ${CMAKE_COMMAND} -E make_directory "${STANDALONE_DIR}"
    COMMAND ${CMAKE_COMMAND} -E make_directory "${STANDALONE_SRC_DIR}"
    COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:winxound>" "${STANDALONE_DIR}/winxound"
    COMMAND ${CMAKE_COMMAND} -E copy ${WINXOUND_UI_FILES} "${STANDALONE_SRC_DIR}"
    COMMAND ${CMAKE_COMMAND} -E copy ${WINXOUND_PNG_FILES} "${STANDALONE_SRC_DIR}"
    COMMAND ${CMAKE_COMMAND} -E copy "${CMAKE_CURRENT_SOURCE_DIR}/src/opcodes.txt" "${STANDALONE_SRC_DIR}/opcodes.txt"
    DEPENDS winxound
    VERBATIM
)