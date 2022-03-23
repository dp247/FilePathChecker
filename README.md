<img width="674" alt="4" src="https://user-images.githubusercontent.com/9065463/126277663-15c31be6-359e-40f7-81f6-8d9016ed7108.png">

# File Path Checker
File Path Checker is a command-line application designed to check the validity of a list of file paths. It's designed to take the monotony out of checking if thousands of files exist by repeatedly calling .NET's File.Exists function for each line of an input file.

The program will  parse the input file into two lists - valid files, where the file path exists and can be located; and invalid files, where the file cannot be found.
