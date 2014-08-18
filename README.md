.Net-Async-IO-Tests
===================

This repository contains the source code for the tests used on the "Tests and thoughts on asynchronous IO vs. multithreading" article. The reporisotry consists of two projects. ParallelUrlRetrieve contains the actual tests and LongRunningRequest is an extremly basic ASP .Net MVC application which simulates long running requests.

A very important aspect about the code inside this repository is that it was created solely for research purposes. There are a lot of aspects about that way it is written that don't make it fit for a production application so use it just for study / inspiration.

The tests were designed to be run directly from Visual Studio simply by uncommenting the ones that were of interest.

The article for which these tests were created can be found at: http://www.ducons.com/blog/tests-and-thoughts-on-asynchronous-io-vs-multithreading
