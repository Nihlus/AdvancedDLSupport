Library Scanning and Path Resolution
====================================

An important thing gotcha with DLSupport is that the library, by default, uses an extended library search pattern 
contrary to what one might be used to in "normal" circumstances. This is done in an attempt to make it easier for the 
end user to distribute multi-architecture assemblies with as  little boilerplate code as possible, and in order to 
simplify distribution of standalone applications.

You can override this behaviour if it does not suit your application - see [Advanced Configuration][1] for more 
information.

In short, DLSupport follows, by default, this search order:

  1. If the library name is actually a path (absolute or relative), the given path (`/some/path/to/libMyLib.so`)
  2. The directory of the entry assembly
     1. The library directory (`<entryDir>/lib`)
     2. The platform-specific directory library (`<entryDir>/lib/x64`) or (`<entryDir>/lib/x86`)
  3. The directory of the executing assembly
     1. The library directory (`<execDir>/lib`)
     2. The platform-specific directory library (`<execDir>/lib/x64`) or (`<execDir>/lib/x86`)
  2. The current directory (`<currDir>/`)
     1. The library directory (`<currDir>/lib`)
     2. The platform-specific directory library (`<currDir>/lib/x64`) or (`<currDir>/lib/x86`)
  3. The platform-specific search pattern
  4. 5. If running under Mono, and the library name is `__Internal`, returns the main assembly
  
At `3.`, the search diverges based on the platform that the library is running on. From here on out, DLSupport follows 
its own implementation of the platform's search implementation.

### Linux

  1. The paths listed in `LD_LIBRARY_PATH`
  2. The cached libraries listed in `/etc/ld.so.cache`
  3. `/lib`
  4. `/usr/lib/`

### Mac
The paths listed in:

  1. `DYLD_FRAMEWORK_PATH`
  2. `DYLD_LIBRARY_PATH`
  3. `DYLD_FALLBACK_FRAMEWORK_PATH`
  4. `DYLD_FALLBACK_LIBRARY_PATH`

### Windows

  1. The local executable directory (`.\ `)
  2. `%WINDIR%\System32`
  3. `%WINDIR%\System`
  4. `%WINDIR%\`
  5. The current working directory
  6. The paths listed in `PATH`


[1]: advanced_config.md
