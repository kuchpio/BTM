## Overview

This project is a result of a series of college assignments based around a small dataset, which represents a fictional public transportation system called BTM. The project was supposed to teach students about object-oriented design. Therefore it utilises design patterns such as adapter, builder, iterator and many others, as well as avoids using built-in mechanisms (like reflection) and instead uses self-written counterparts like collections and lambda expressions. 

The program is a CLI that allows managing initial dataset. The terminal can work in two modes **immediate** and **queue**. The only difference between the two is that in queue mode some commands are pushed to the queue after being called and are executed only after commiting the queue. The mode depends on the value of an agrument passed to
`Terminal.Run(bool executeImmediately)`.

## Usage

#### Requirements

 * .NET 7.0
 
#### Running

Simply clone this repository, navigate to it's root folder and call
```sh
> dotnet run
```

A CLI with a prompt `BTM (?) > ` (`?` represents current mode) should appear. To learn more about how everything works use commands `help` and `help <command>`.

## Other information

This project won't be maintained further. Feel free to use it under [MIT](https://mit-license.org/) License. 
