cmake_minimum_required(VERSION 3.5)
project(c)

set(CMAKE_C_STANDARD 11)
set(CMAKE_CXX_STANDARD 11)

if ( CMAKE_COMPILER_IS_GNUCC )
	set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -Wall -Wno-long-long -pedantic")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wno-long-long -pedantic")
endif()
if ( MSVC )
	set(CMAKE_CXX_FLAGS "${CMAKE_C_FLAGS} /W4")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W4")
endif()

include_directories(${CMAKE_SOURCE_DIR})

set(SHARED_HEADERS src/TestStruct.h)

add_library(BaseTests SHARED src/BaseTests.c ${SHARED_HEADERS})
add_library(DisposeTests SHARED src/DisposeTests.c ${SHARED_HEADERS})
add_library(EventTests SHARED src/EventTests.c ${SHARED_HEADERS})
add_library(FunctionTests SHARED src/FunctionTests.c ${SHARED_HEADERS})
add_library(LazyLoadingTests SHARED src/LazyLoadingTests.c ${SHARED_HEADERS})
add_library(PropertyTests SHARED src/PropertyTests.c ${SHARED_HEADERS})
add_library(RemappingTests SHARED src/RemappingTests.c ${SHARED_HEADERS})

install(TARGETS BaseTests DisposeTests EventTests FunctionTests LazyLoadingTests PropertyTests RemappingTests
        DESTINATION ${CMAKE_BINARY_DIR}/lib)