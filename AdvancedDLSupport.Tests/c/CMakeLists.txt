cmake_minimum_required(VERSION 3.5)
project(c)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

set(bitness 32)
if(CMAKE_SIZEOF_VOID_P EQUAL 8)
    set(bitness 64)
endif()

set(TARGET_PLATFORM "x${bitness}" CACHE STRING "The target platform.")

if (TARGET_PLATFORM STREQUAL "AnyCPU")
    set(TARGET_PLATFORM "x${bitness}")
endif()

set(OUTPUT_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The path where the output binaries should be installed.")
set(PROJECT_PATH "${CMAKE_BINARY_DIR}/lib" CACHE STRING "The path where the output binaries should be installed.")

set(CMAKE_GENERATOR_PLATFORM ${TARGET_PLATFORM})

if (CMAKE_COMPILER_IS_GNUCC OR CMAKE_C_COMPILER_ID MATCHES "Clang")
    set(IS_UNIX_COMPILER true)
endif()

if ( IS_UNIX_COMPILER )
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
elseif ( MSVC )
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
endif()

include_directories(${CMAKE_SOURCE_DIR})

set(SHARED_HEADERS src/TestStruct.h src/comp.h)

# Uncomment if testing on a non-unicode platform
add_definitions(-DUNICODE)

add_library(BaseTests SHARED src/BaseTests.c ${SHARED_HEADERS})
add_library(BaseTests-x64 SHARED src/BaseTests.c ${SHARED_HEADERS})
add_library(BaseTests-x32 SHARED src/BaseTests.c ${SHARED_HEADERS})
add_library(DisposeTests SHARED src/DisposeTests.c ${SHARED_HEADERS})
add_library(EventTests SHARED src/EventTests.c ${SHARED_HEADERS})
add_library(FunctionTests SHARED src/FunctionTests.c ${SHARED_HEADERS})
add_library(LazyLoadingTests SHARED src/LazyLoadingTests.c ${SHARED_HEADERS})
add_library(PropertyTests SHARED src/PropertyTests.c ${SHARED_HEADERS})
add_library(RemappingTests SHARED src/RemappingTests.c ${SHARED_HEADERS})
add_library(TypeLoweringTests SHARED src/TypeLoweringTests.c ${SHARED_HEADERS})
add_library(AttributePassthroughTests SHARED src/AttributePassthroughTests.c ${SHARED_HEADERS})
add_library(MixedModeTests SHARED src/MixedModeTests.c ${SHARED_HEADERS})
add_library(NullableTests SHARED src/NullableTests.c ${SHARED_HEADERS})

if ( IS_UNIX_COMPILER )
    set_target_properties(BaseTests-x64 PROPERTIES COMPILE_FLAGS "-m64" LINK_FLAGS "-m64")
    set_target_properties(BaseTests-x32 PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
elseif ( MSVC )
    # TODO: Do something to force a 32-bit build on BaseTests-x32
endif()

get_filename_component(INSTALL_PATH_ABSOLUTE "${PROJECT_PATH}/${OUTPUT_PATH}" ABSOLUTE)

install(TARGETS
            BaseTests
            DisposeTests
            EventTests
            FunctionTests
            LazyLoadingTests
            PropertyTests
            RemappingTests
            TypeLoweringTests
            AttributePassthroughTests
            MixedModeTests
            NullableTests
        COMPONENT
            standard
        DESTINATION
            ${INSTALL_PATH_ABSOLUTE})

install(TARGETS
            BaseTests-x64
        COMPONENT
            standard
        DESTINATION
            ${INSTALL_PATH_ABSOLUTE}/lib/x64)

install(TARGETS
            BaseTests-x32
        COMPONENT
            x32-only
        DESTINATION
            ${INSTALL_PATH_ABSOLUTE}/lib/x32)
