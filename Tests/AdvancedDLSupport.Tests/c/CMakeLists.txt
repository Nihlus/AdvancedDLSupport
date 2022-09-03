cmake_minimum_required(VERSION 3.7)
project(c)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

set(TARGET_FRAMEWORKS "net6.0" "net461" CACHE STRING "The target frameworks")
set(INSTALL_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The installation path")

if (CMAKE_COMPILER_IS_GNUCC OR CMAKE_C_COMPILER_ID MATCHES "Clang")
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
elseif (MSVC)
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
endif()

list(
    APPEND StandardTargets
        BaseTests
        DisposeTests
        EventTests
        FunctionTests
        LazyLoadingTests
        PropertyTests
        RemappingTests
        StringTests
        AttributePassthroughTests
        MixedModeTests
        NullableTests
        IndirectCallTests
        NameManglingTests
        SymbolTransformationTests
        GenericDelegateTests
        BooleanMarshallingTests
        SpanMarshallingTests
)

if(UNICODE)
    add_compile_definitions(UNICODE)

    if(MSVC)
        add_compile_options("/utf-8")
    endif()
endif()

foreach (target ${StandardTargets})
	add_library(${target} SHARED src/${target}.c)
    target_include_directories(${target}
        PRIVATE
            ${CMAKE_SOURCE_DIR}
    )
endforeach(target)

get_filename_component(INSTALL_PATH_ABSOLUTE "${INSTALL_PATH}" ABSOLUTE)

foreach(framework ${TARGET_FRAMEWORKS})
    install(TARGETS
            ${StandardTargets}
        DESTINATION
            ${INSTALL_PATH_ABSOLUTE}/${framework}
    )
endforeach()
