#include <iostream>

using namespace std;

int main()
{
    int k;
    int i;
    int j;

    pair<pair<int, int>, pair<int, int>> Array[100];

    cin >> k;

    int minColumn = 0;
    int minLine = 0;
    int columnMatches = 0;
    int lineMatches = 0;

    for (int index = 0; index < k; index++)
    {
        cin >> i;
        cin >> j;

        for (int jindex = 0; jindex <= index - lineMatches; jindex++)
        {
            if (Array[jindex].first.first == i)
            {
                lineMatches++;
                Array[jindex].first.second++;
                minLine = Array[jindex].first.second;
            }
            else
            {
                Array[index - lineMatches].first.first = i;
                Array[index - lineMatches].first.second = 0;
            }
        }

        for (int jindex = 0; jindex <= index - columnMatches; jindex++)
        {
            if (Array[jindex].second.first == j)
            {
                columnMatches++;
                Array[jindex].second.second++;
                minColumn = Array[jindex].second.second;
            }
            else
            {
                Array[index - lineMatches].second.first = j;
                Array[index - lineMatches].second.second = 0;
            }
        }

        minColumn > minLine ? cout << minColumn << "\n" : cout << minLine << "\n";
    }
}
