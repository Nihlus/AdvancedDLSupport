Complex Types
=============

AdvancedDLSupport also allows you to use more complex types than
traditional P/Invoke. Chief among these are the following, which can be
used just like you'd use them in C#.

* `string`
* `Nullable<T>`
* `bool`

Complex types are handled much in the same way as compilers handle
syntactic sugar (or "compiler magic") - custom [compiler lowering][2])
by generating automatic wrapper constructs that take care of all the
tedious marshalling and interoperation for you.

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

```cs
public interface IPulseSimple
{
    IntPtr pa_simple_new
    (
        string server,
        string name,
        StreamDirection dir,
        string dev,
        string stream_name,
        SampleSpecification ss,
        ChannelMap? map,
        BufferingAttributes? attr,
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

One important thing to note is that `Nullable<T>` does not yet support the `ref` or `out` modifier.

[1]: https://freedesktop.org/software/pulseaudio/doxygen/simple_8h_source.html
[2]: http://mattwarren.org/2017/05/25/Lowering-in-the-C-Compiler