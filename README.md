# Book Cover Downloader

A simple command-line utility to automatically download missing book covers via their ISBN numbers via [OpenLibrary](https://openlibrary.org/).

## Description

This utility is used to acquire small and medium sized cover art for various books in relation to another project in progress (a book database similar to GoodReads or StoryGraph).
The program communicates with a database (currently SQLServer) to obtain a list of ISBN's that do not have an associated cover on record.

Once a list of ISBN's has been obtained, the program will iterate through the list and query OpenLibrary for an available record for the ISBN and the associating Author.
The program will terminate an ISBN query if there is no record found for either the ISBN or the Author, as both information are currently required to neatly organize records on disk.

## Getting Started

### Dependencies

* .NET 9

## Authors

[Jordan Gough](https://jgoughportfolio.weebly.com/)

## License

This project is licensed under the Apache-2.0 License - see the LICENSE.md file for details

