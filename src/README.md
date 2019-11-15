
## Important build instructions

When (re)compiling the DevStudio solution via any of the `Build` or `Rebuild All` commands you may encounter the following error for one or more of the DLLs and the build process will fail:

```
ALINK : error AL1078: Error signing assembly -- Access is denied.
```

The solution here is to give yourself Full Control access to the dev machine's `NachineKeys` folder as per https://stackoverflow.com/questions/4606342/signing-assembly-access-is-denied, in particular: https://stackoverflow.com/questions/4606342/signing-assembly-access-is-denied#answer-4606691 and the comments there:

> I don't know if it's Window 7 or the company policy, but I had to take ownership of the `C:\Users\All Users\Application Data\Microsoft\Crypto\RSA\MachineKeys` folder and give myself full control. This corrected the issue.
> 
> Risho answered Jan 5 '11 at 16:44
>
> Yep, this definitely works. Obnoxious problem. All you need is 'This folder only' set to 'Full Control'. – user7116 Sep 27 '12 at 17:06
> 
> Using Windows 10 the folder differed slightly: I had to add the rights to C:\Users\All Users\Microsoft\Crypto\RSA\MachineKeys (no Application Data), but otherwise it worked as described. – SJP May 10 '16 at 11:03
> 
> @SJP FYI, C:\Users\All Users is an alias for C:\ProgramData which is visible without showing hidden system files. – mjohnsonengr May 16 '16 at 15:20 
> 
> For me it was 'C:\ProgramData\Microsoft\Crypto\RSA\MachineKeys' but the same principle worked. – Casey O'Brien Aug 10 '18 at 19:48
> 
(SO link found by way of https://github.com/dotnet/corefx/issues/3784 issue comments)
