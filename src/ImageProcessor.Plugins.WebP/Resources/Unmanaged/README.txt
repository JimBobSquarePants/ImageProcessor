Build instructions for libwebp.dll 
==================================


Download libwebp-{version}.tar.gz from the downloads list at http://downloads.webmproject.org/releases/webp 
and extract its contents.

In Start Menu, run Visual Studio Tools>Command Prompt.
C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\Shortcuts\Developer Command Prompt for VS2013

Change to the libwebp-{version} directory, run:

nmake /f Makefile.vc CFG=release-dynamic RTLIBCFG=dynamic OBJDIR=output

Repeat with the x64 Cross Tools Command Prompt.
C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\Shortcuts\VS2013 x64 Cross Tools Command Prompt

Copy to x86 and x64 directories from /output/bin/

To verify p/invokes have not changed:

Review the following history logs for changes since the last release:

http://git.chromium.org/gitweb/?p=webm/libwebp.git;a=history;f=src/webp/types.h;hb=HEAD
http://git.chromium.org/gitweb/?p=webm/libwebp.git;a=history;f=src/webp/encode.h;hb=HEAD
http://git.chromium.org/gitweb/?p=webm/libwebp.git;a=history;f=src/webp/decode.h;hb=HEAD