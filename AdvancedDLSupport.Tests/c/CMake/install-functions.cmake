function(install_for_frameworks FRAMEWORKS)
    foreach(framework ${FRAMEWORKS})
        message("Framework: ${framework}")
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
                    IndirectCallTests
                    NameManglingTests
                COMPONENT
                    standard
                DESTINATION
                    ${INSTALL_PATH_ABSOLUTE}/${framework})

        install(TARGETS
                    BaseTests-x64
                COMPONENT
                    x64-only
                DESTINATION
                    ${INSTALL_PATH_ABSOLUTE}/${framework}/lib/x64)

        install(TARGETS
                    BaseTests-x32
                COMPONENT
                    x32-only
                DESTINATION
                    ${INSTALL_PATH_ABSOLUTE}/${framework}/lib/x32)
    endforeach()
endfunction()
