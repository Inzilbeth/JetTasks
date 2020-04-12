#include <iostream>
#include <math.h>

using namespace std;

int main()
{
    int k;
    int i;
    int j;

    int multiplier[4][4] = { 0,1,2,3,1,0,3,2,2,3,0,1,3,2,1,0 };

    cin >> k;

    for (int index = 0; index < k; index++)
    {
        cin >> i;
        cin >> j;

        int tempI = i;
        int tempJ = j;
        int maxPower = 0;
        int result = 0;

        if (i == 0 && j == 0)
        {
            cout << 0;
            continue;
        }

        while (tempI != 0 || tempJ != 0)
        {
            tempI /= 4;
            tempJ /= 4;
            maxPower++;
        }

        maxPower--;

        for (int e = maxPower; e >= 0; e--)
        {
            int coefficient = pow(4, e);

            int localI = i / coefficient;
            i %= coefficient;

            int localJ = j / coefficient;
            j %= coefficient;

            result += (coefficient * multiplier[localI][localJ]);
        }

        cout << result;
    }
}
