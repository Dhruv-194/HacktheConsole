#include <iostream>
#include <Windows.h>

using namespace std;

int main()
{

    STARTUPINFO startInfo = {0};

    PROCESS_INFORMATION processInfo = {0};

    STARTUPINFO startInfo2 = {0};

    PROCESS_INFORMATION processInfo2 = {0};

   int n;
    cout<< "Press '1' to play Console 2048"<<endl;
    cout<<"Press '2' to play Console MineSweeper"<<endl;
    cin>>n;

    switch (n){

    case 1:
       {
        cout<<"You are now playing Flappy Bird"<<endl;
        BOOL bSuccess = CreateProcess(TEXT("C:\\Users\\hp\\Downloads\\REPO\\REPO\\Game\\2048\\Console2048.exe"),NULL,NULL,NULL,FALSE,NULL,NULL,NULL, &startInfo,&processInfo);
        break;
       }
    case 2:
        cout<<"You are now playing car race"<<endl;
        BOOL bSuccesss = CreateProcess(TEXT("C:\\Users\\hp\\Downloads\\REPO\\REPO\\Game\\Minesweeper\\ConsoleMinesweep.exe"),NULL,NULL,NULL,FALSE,NULL,NULL,NULL, &startInfo2,&processInfo2);
        break;

    }

return 0 ;

}
