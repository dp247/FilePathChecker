# File Path Checker
The aptly named File Path Checker is a utility I wrote that just pings .NET's File.Exists function for a list of file paths to save you having to check them manually. That's it, it's just a fancy wrapper.

## But why?
The program was born out of a need to check thousands of file paths stored in an SQL Server database (document attachments that may or may not exist on a file server, as "house keeping was very intermitent", as my manager put it). We needed a list of files that definitely existed, as well as when they were last modified to see what could stay and what could go. Rather than spend hours and hours manually checking each file path, I made this utility, and have used it more than I planned to since (turns out there's a lot of files that may or may not exist).
