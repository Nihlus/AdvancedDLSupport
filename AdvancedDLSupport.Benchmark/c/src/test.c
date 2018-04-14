#include "comp.h"

typedef struct Vector2
{
    float X;
    float Y;
} Vector2;

typedef struct Matrix2
{
    Vector2 Row0;
    Vector2 Row1;
} Matrix2;

__declspec(dllexport) void InvertMatrixByPtr(Matrix2* matrix)
{
    // Calculate determinant over one
    float det = 1 / ((matrix->Row0.X * matrix->Row1.Y) - (matrix->Row0.Y * matrix->Row1.X));

    float tmpd = matrix->Row1.Y;

    // Swap d and a
    matrix->Row1.Y = matrix->Row0.X;
    matrix->Row0.X = tmpd;

    // Negate b and c
    matrix->Row0.Y = -matrix->Row0.Y;
    matrix->Row1.X = -matrix->Row1.X;

    // And multiply by the determinant modifier
    matrix->Row0.X *= det;
    matrix->Row0.Y *= det;
    matrix->Row1.X *= det;
    matrix->Row1.Y *= det;
}

__declspec(dllexport) Matrix2 InvertMatrixByValue(Matrix2 matrix)
{
    InvertMatrixByPtr(&matrix);
    return matrix;
}
