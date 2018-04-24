Complex Types
=============

AdvancedDLSupport also allows you to easily use more complex types than traditional P/Invoke. Chief among these are the
following, which can be used just like you'd use them in C#.

* `string`
* `Nullable<T>`
* `bool`

Complex types are handled much in the same way as compilers handle syntactic sugar (or "compiler magic") - custom
[compiler lowering][2]) by generating automatic wrapper constructs that take care of all the tedious marshalling and
interoperation for you.

Thus, constructs like this is a simple matter for AdvancedDLSupport:

Taken from the [PulseAudio source code][1]
```c
typedef struct pa_simple pa_simple;

pa_simple* pa_simple_new(
    const char *server,
    const char *name,
    pa_stream_direction_t dir,
    const char *dev,
    const char *stream_name,
    const pa_sample_spec *ss,
    const pa_channel_map *map,
    const pa_buffer_attr *attr,
    int *error
    );

void pa_simple_free(pa_simple *s);

int pa_simple_write(pa_simple *s, const void *data, size_t bytes, int *error);

int pa_simple_drain(pa_simple *s, int *error);

int pa_simple_read(
    pa_simple *s,
    void *data,
    size_t bytes,
    int *error
    );

pa_usec_t pa_simple_get_latency(pa_simple *s, int *error);

int pa_simple_flush(pa_simple *s, int *error);
```

```c#
public interface IPulseSimple
{
    IntPtr pa_simple_new
    (
        string server,
        string name,
        StreamDirection dir,
        string dev,
        string stream_name,
        ref SampleSpecification ss,
        ref ChannelMap? map,
        ref BufferingAttributes? attr,
        out int error
    );

    void pa_simple_free(IntPtr s);

    int pa_simple_write(IntPtr s, byte[] data, UIntPtr bytes, out int error);

    int pa_simple_drain(IntPtr s, out int error);

    int pa_simple_read
    (
        IntPtr s,
        byte[] data,
        UIntPtr bytes,
        out int error;
    );

    ulong pa_simple_get_latency(IntPtr s, out int error);

    int pa_simple_flush(IntPtr s, out int error);
}
```

Of note is string handling, which traditional P/Invoke handles via the `MarshalAs` attribute. ADL also respects the
`MarshalAs` attribute, and defaults to marshalling string parameters as `uchar8_t*`. The typical `StringBuilder` construct
is also supported.

When running under Mono, `LPTSTR` is handled as a unicode string.

```c#
public interface IMyStringLibrary
{
    string GetSomeString();

    int GetWStrLength([MarshalAs(UnmanagedType.LPWStr)] string uniString);

    [return: MarshalAs(UnmanagedType(LPWStr)]
    string GetSomeWString();
}
```

You, the developer, can also decide who should clean up the unmanaged memory allocated for the string. By annotating a
parameter or return value with the `CallerFree` attribute, ADL will automatically free the unmanaged memory for the 
string that was allocated while marshalling.

```c#
[return: CallerFree]
string GetStringAndFree();

UIntPtr GetStringLengthAndFree([CallerFree] string value);
```

[1]: https://freedesktop.org/software/pulseaudio/doxygen/simple_8h_source.html
[2]: http://mattwarren.org/2017/05/25/Lowering-in-the-C-Compiler
