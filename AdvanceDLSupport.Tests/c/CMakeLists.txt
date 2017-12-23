cmake_minimum_required(VERSION 3.9)
project(c)

set(CMAKE_C_STANDARD  11)
set(CMAKE_CXX_STANDARD 11)

set(CMAKE_C_COMPILER /usr/bin/clang)
set(CMAKE_CXX_COMPILER /usr/bin/clang++)

if(MSVC)
    # Force to always compile with W4
    if(CMAKE_CXX_FLAGS MATCHES "/W[0-4]")
        string(REGEX REPLACE "/W[0-4]" "/W4" CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS}")
    else()
        set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
    endif()
elseif(CMAKE_COMPILER_IS_GNUCC OR CMAKE_COMPILER_IS_GNUCXX)
    # Update if necessary
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
endif()

set(CMAKE_LIBRARY_OUTPUT_DIRECTORY  ${CMAKE_BINARY_DIR}/lib)

include_directories(${CMAKE_SOURCE_DIR})

set(SHARED_HEADERS src/TestStruct.h)

add_library(BaseTests SHARED src/BaseTests.c ${SHARED_HEADERS})
add_library(DisposeTests SHARED src/DisposeTests.c ${SHARED_HEADERS})
add_library(EventTests SHARED src/EventTests.c ${SHARED_HEADERS})
add_library(FunctionTests SHARED src/FunctionTests.c ${SHARED_HEADERS})
add_library(LazyLoadingTests SHARED src/LazyLoadingTests.c ${SHARED_HEADERS})
add_library(PropertyTests SHARED src/PropertyTests.c ${SHARED_HEADERS})
