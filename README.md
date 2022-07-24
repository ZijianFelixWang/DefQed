# DefQed
![](https://img.shields.io/github/license/ZijianFelixWang/DefQed?style=flat-square)
![](https://img.shields.io/github/languages/code-size/ZijianFelixWang/DefQed?style=flat-square)
![](https://img.shields.io/tokei/lines/github/ZijianFelixWang/DefQed?style=flat-square)
![](https://img.shields.io/codacy/grade/135e566bb35047bf9c9bf07dcaa0a069?style=flat-square)

## What's This?
**DefQed** is a statement prover with its own algorithm for academic written in C#. It is immature and still in active (and very early) development.

## What license does it use?
The BSD 3-Clause "New" or "Revised" License. For more information, see the `LICENSE` file.

The Mathematica notebook inside use a Creative Commons license. Open it for further information.

## What can it do?
Currently it can prove statements described in the input XML file with essential 'knowledge' installed in the database. What's special, it does **not** contain any reflections (i.e. rules) itself, even the simplest `(x==y&&y==z)=>(x==z)` ones.

### How can it do such?
Well, for this problem, please refer to `/DefQed/Documents/Algorithm.nb` in Mathematica form.

**Note.** Continuing, we'll use some terms explained there.

### Can it prove xxx?
Maybe not yet. It is in **very** early stage now. Many things're not implemented. Although no knowledge is predefined, the algorrithm still has many problems so it may fail to do many proof now. (I'm trying to improve such though.)

## Some of the components
- *Bracket*: An effective way to express math for computers.
- *Prover*: It can current can prove very simple ones.
- *MySQLDriver*: To connect to the KnowledgeBase.
- *XMLParser*: To extract statements from XML form input.
- *JSDriver*: To extract statements from JS form input (beta).

For unimplemented ones, see `/DefQed/Documents/Roadmap.md`.

## Usage
For a quick intro, go to Get Started section.

### Requirements
The program is currently only tested in Windows. However, further support on Linux and macOS will be added.
- *.NET 6.0 runtime*: You only need the basic one as it's a console application.
- *MySQL Database*: The program store data in it so such is necessary.

### Releases
- `v0.01`: Experimental but basically working version. Only offer Windows binary in release page.
- `v0.02`: Under active development now.

### Build
Load the `.sln` file in Visual Studio (recommend: version 2022) and perform build.

You can also use `msbuild` to do it manually. For example, if you only want to build `DefQed.csproj`, just run

```PowerShell
msbuild.exe ./DefQed.csproj
```
or
```PowerShell
dotnet msbuild ./DefQed.csproj
```
or
```PowerShell
dotnet build
```
under the `/DefQed/DefQed/` directory.

**Note.** Unless you want to develop the program, choose `Release` template instead of `Debug`. There are differences,

### Get Started (v0.01)
**Note.** This part will be removed (or moved otherwhere) later. Refer to the Get Started (v0.02) section.

**Note.** You may want to add it to `PATH`: `DefQed.exe`

DefQed is a console program. Command line:
```PowerShell
./DefQed.exe file.xml
```

**Note.** The command above doesn't work now. Use:
```PowerShell
./DefQed.exe -l=Information -f=xml file.xml
```
where `Information` (represents log level) can be replaced with `Diagnostic`, `Warning`, `Error`.

**Note.** If you are using a `Release` build, `Information` is default; if you are using a `Debug` build, `Diagnostic` is default. This parameter is optional.

**Note.** The format parameter (either `xml` or `js`) has default value `xml` since JavaScript feature is experimental now.

Wait... Before executing such, you need to configure the database. In MySQL, create a new database and a new user (and a password) for the program to use. Then create all of these tables:
```SQL
+------------------+
| Tables_in_defqed |
+------------------+
| notations        |
| reflections      |
| registries       |
+------------------+
+---------+-------------+------+-----+---------+-------+
| Field   | Type        | Null | Key | Default | Extra |
+---------+-------------+------+-----+---------+-------+
| ID      | int         | NO   | PRI | NULL    |       |
| TITLE   | varchar(50) | NO   |     | NULL    |       |
| ORIGIN  | tinyint     | NO   |     | NULL    |       |
| OPACITY | double      | NO   |     | NULL    |       |
+---------+-------------+------+-----+---------+-------+
+---------+--------+------+-----+---------+-------+
| Field   | Type   | Null | Key | Default | Extra |
+---------+--------+------+-----+---------+-------+
| ID      | int    | NO   | PRI | NULL    |       |
| CASES   | bigint | NO   |     | NULL    |       |
| THUSES  | bigint | NO   |     | NULL    |       |
| OPACITY | double | NO   |     | NULL    |       |
+---------+--------+------+-----+---------+-------+
+---------+----------+------+-----+---------+-------+
| Field   | Type     | Null | Key | Default | Extra |
+---------+----------+------+-----+---------+-------+
| ID      | int      | NO   | PRI | NULL    |       |
| CONTENT | longtext | NO   |     | NULL    |       |
+---------+----------+------+-----+---------+-------+
```
For diagnostic purpose, you can import the *diagnostic database* from `/DefQed/Examples/diagnostic.sql`.

Next, open the file `/DefQed/Examples/Diagnostic.xml` and change the things in
```XML
<connection>
		<user>DefQed</user>
		<passwd>oClg2%[TenbL86V+rsC3</passwd>
		<database>defqed</database>
</connection>
```
to your settings. Then, execute the program with the commandline usage above. If there's no problem, you will finally see:
```txt
DefQed.                                                                                                                                   
[INFORMATION][5/9/2022 20:15:56] OS: Microsoft Windows 10.0.22616                                                                         
[INFORMATION][5/9/2022 20:15:57] Connection state: Open                                                                                   
[INFORMATION][5/9/2022 20:15:57] XML parsing done in 483 ms.                                                                              
[INFORMATION][5/9/2022 20:15:57] Congratulations: XML parse and DB connect ok.                                                            
[INFORMATION][5/9/2022 20:15:57] Start proving...                                                                                         
[INFORMATION][5/9/2022 20:15:57] Loading reflections.                                                                                     
[   DEBUG   ][5/9/2022 20:15:57] Deserializing reflection 0.                                                                              
[   DEBUG   ][5/9/2022 20:15:57] Reflection 1 loaded.                                                                                     
...                                                                                  
[   DEBUG   ][5/9/2022 20:15:58] Scan: Pool updated.                                                                                      
[   DEBUG   ][5/9/2022 20:15:58] Pool Content: MicroStatement(Bracket(x); == Bracket(z);                                                  
[   DEBUG   ][5/9/2022 20:15:58] ScanPools: ReflectionHistory is hiden. RH size is 7368                                                   
[INFORMATION][5/9/2022 20:15:58] Proof process has finished.                                                                              
                                                                                                                                          
Generate report file?                                                                                                                     
N/ENTER = No; S = Serialized KBase; T = Proof text; B = Both                                                                             
```
Then you can follow the instructions to gain a report like this:
```txt
Proof(
	Scan(Reflection = Reflection(Condition = Formula(Bracket(Satisfied = True, Type = BracketHolder,
	Bracket(Satisfied = True, Type = StatementHolder,
	MicroStatement(Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([0]ITEM);, [0](A), Value = );); Notation([1]==); Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([0]ITEM);, [1](B), Value = );););); Notation([2]AND); Bracket(Satisfied = True, Type = StatementHolder,
	MicroStatement(Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([1]ITEM);, [1](B), Value = );); Notation([1]==); Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([2]ITEM);, [2](C), Value = ););););););, Conclusions = List(MicroStatement(Bracket(Satisfied = Unknown, Type = SymbolHolder,
	...
	Symbol(Notation = Notation([0]ITEM);, [0](X), Value = );); Notation([1]==); Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([0]ITEM);, [2](Z), Value = );););
););
```
Well, I admit it's a bit hard for humans to read... It'll be improved later,

**Note.** The program is experimental. Many of the logs're for debug purposes and will be rermoved in further version.

### I've encounted a problem/error/exception/etc.
- If you encountered a warning about operating systems, just ignore it.
- If you encountered a XML parse failure like:
```txt
DefQed.
[INFORMATION][5/9/2022 20:25:17] OS: Microsoft Windows 10.0.22616
[   ERROR   ][5/9/2022 20:25:17] Given XML file doesn't exist.
Could not find file 'C:\Users\felix\Downloads\DefQed-v0.01\Examples\Diagnostic.xm'.
[INFORMATION][5/9/2022 20:25:17] XML parsing done in 30 ms.
[   ERROR   ][5/9/2022 20:25:17] XML parse failure.
[INFORMATION][5/9/2022 20:25:17] Start proving...
[INFORMATION][5/9/2022 20:25:17] Loading reflections.
[   ERROR   ][5/9/2022 20:25:17] Exception envountered. Exception Category: System.InvalidOperationException
Exception Message: Connection must be valid and open.
You can now save the dump file or terminate the program.
Asking: (Json Serialized KBase) File name:
```
This means the file you typed cannot be accessed. There may be a typo in your parameters.
- If you encountered other XML parse failure, check if your source file is correct.
- Other issue. There may be a **bug** please check the **Known issues** section below. You are welcome to inspect and you can contribute to improve it.

### The XML file format
Below is an example: `diagnostic.xml`
```XML
<?xml version="1.0" encoding="utf-8" ?> 
<!-- Use the defqed diagnostic database for this. -->
<defqed>
	<connection>
		<user>DefQed</user>
		<passwd>oClg2%[TenbL86V+rsC3</passwd>
		<database>defqed</database>
	</connection>
	<environment>
		<enroll category="item">x</enroll>
		<enroll category="item">y</enroll>
		<enroll category="item">z</enroll>

		<force>
			<let category="item">x</let>
			<be category="item">y</be>
		</force>

		<force>
			<let category="item">y</let>
			<be category="item">z</be>
		</force>
	</environment>
	<statement>
		<prove>
			<that category="==">
				<that category="item">x</that>
				<that category="item">z</that>
			</that>
		</prove>
	</statement>
</defqed>
```
The root tag must be `defqed`. It must contain three parts:
- `connection`: See section Get Started's description.
- `environment`: Encodes the `condition` of the stuff-to-prove. It contains `enroll` tags and `force` tags.
- `statement`: Encodes the `conclusion` to check. It contains multiple `prove` tags.

The `enroll` tag is in the following format:
```XML
<enroll category="Symbol-Category">Symbol-Name</enroll>
```
Each `force` tag contains a `let` tag and a `be` tag. Their formats:
```XML
<let category="Symbol-Category">Symbol-Name</let>
<be category="Symbol-Category">Symbol-Name</be>
```
Each `prove` tag contains a tree of `that` tag (which represents a `Bracket`).
```XML
<that category="Bracket-Category">SubTags</that>
```
**Note.** The format above is far from perfect. It will be changed later.

## Get Started (v0.02)
**Note.** This part is *experimental* which means the content will be changed later.

**Note.** You may want to add it to `PATH`: `DefQed.exe`

DefQed is a command line program. Usage:
```PowerShell
./DefQed.exe [-l LogLevel] [-f Format] filename
```

Explanation of the command line options and arguments:
- `LogLevel`: `Diagnostic`, `Information`, `Warning`, `Error`. If you are using a `Release` build, `Information` is default; if you are using a `Debug` build, `Diagnostic` is default. This parameter is optional.
- `Format`: Either `xml` or `js`. If you select `xml`, refer to Get Started (v0.02) section, if you select `js`, see below.
- `filename`: Either a XML file or a JavaScript file.

### Installation
**Note.** While there is a project under solution called `Installer`, it is far from completion so that you must install it manually now.

First, install the utilities and services under Requirements section. Next, either build the program (see the Build section) or download a release (now only v0.01) from GitHub then execute.

### Setup the diagnostic database
Before experimenting with the algorithm, you need to configure the database. The most convenient approach is to import the `/DefQed/Examples/diagnostic.sql` file into the database. Or, you can follow the manual instructions in the Get Started (v0.01) section.

### Modify the JavaScript file
The JavaScript feature makes things simple and convenient. Open the JS file provided in `/DefQed/Examples/Diagnostic.js` and find the line
```JavaScript
JSDriver.Connect("DefQed", "oClg2%[TenbL86V+rsC3", "defqed");
```
modify the parameters in the form:
```JavaScript
JSDriver.Connect("username", "password", "database");
```

### Play with the execution
After configuring the JavaScript file, execute the program with the just-modified JavaScript and you will see the output below if there's no problem.

```txt
DefQed. For detailed help, refer to github.
[INFORMATION][7/24/2022 11:09:53] OS: Microsoft Windows 10.0.25158
[INFORMATION][7/24/2022 11:09:53] Set LogLevel as    DEBUG   .
[  WARNING  ][7/24/2022 11:09:53] LoadJS feature is EXPERIMENTAL.
[INFORMATION][7/24/2022 11:09:53] Start to execute JavaScript.
[INFORMATION][7/24/2022 11:09:54] Connection state: Open
[INFORMATION][7/24/2022 11:09:54] JavaScript execution done.
[INFORMATION][7/24/2022 11:09:54] JS load done in 951 ms
[INFORMATION][7/24/2022 11:09:54] Congratulations: All set up, ready to work.
[INFORMATION][7/24/2022 11:09:54] Start proving...
[INFORMATION][7/24/2022 11:09:54] Loading reflections.
[   DEBUG   ][7/24/2022 11:09:54] Deserializing reflection 0.
[   DEBUG   ][7/24/2022 11:09:54] Reflection 1 loaded.

...
   DEBUG   ][7/24/2022 11:09:56] Same merge. Ignored.
[   DEBUG   ][7/24/2022 11:09:56] Scan: Pool updated.
[   DEBUG   ][7/24/2022 11:09:56] Pool Content: MicroStatement(Bracket(x); == Bracket(z);
[   DEBUG   ][7/24/2022 11:09:56] ScanPools: ReflectionHistory is hiden. RH size is 7368
[INFORMATION][7/24/2022 11:09:56] Proof process has finished.

Generate report file?
N/ENTER = No; S = Serialized KBase; T = Proof text; B = Both
```

Currently, the `js` mode is about 100 ms slower than `xml` mode, but its code and scripting is more convenient and flexible.

Then you can follow the instructions to gain a report like this:
```txt
Proof(
	Scan(Reflection = Reflection(Condition = Formula(Bracket(Satisfied = True, Type = BracketHolder,
	Bracket(Satisfied = True, Type = StatementHolder,
	MicroStatement(Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([0]ITEM);, [0](A), Value = );); Notation([1]==); Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([0]ITEM);, [1](B), Value = );););); Notation([2]AND); Bracket(Satisfied = True, Type = StatementHolder,
	MicroStatement(Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([1]ITEM);, [1](B), Value = );); Notation([1]==); Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([2]ITEM);, [2](C), Value = ););););););, Conclusions = List(MicroStatement(Bracket(Satisfied = Unknown, Type = SymbolHolder,
	...
	Symbol(Notation = Notation([0]ITEM);, [0](X), Value = );); Notation([1]==); Bracket(Satisfied = Unknown, Type = SymbolHolder,
	Symbol(Notation = Notation([0]ITEM);, [2](Z), Value = );););
););
```
Well, I admit it's a bit hard for humans to read... It'll be improved later,

**Note.** The program is experimental. Many of the logs're for debug purposes and will be rermoved in further version.

### I've encounted a problem/error/exception/etc.
- If you encountered a warning about operating systems, just ignore it.
- If you encountered a commandline failure like:
```txt
DefQed. For detailed help, refer to github.
[INFORMATION][7/24/2022 11:20:55] OS: Microsoft Windows 10.0.25158
The FileName field is required.
Specify --help for a list of available options and commands.
```
You are missing the filename argument

- If you encountered a LoadJS failure like:
```txt
DefQed. For detailed help, refer to github.
[INFORMATION][7/24/2022 11:23:09] OS: Microsoft Windows 10.0.25158
[INFORMATION][7/24/2022 11:23:09] Set LogLevel as    DEBUG   .
[  WARNING  ][7/24/2022 11:23:09] LoadJS feature is EXPERIMENTAL.
[   ERROR   ][7/24/2022 11:23:09] Failed to load content from specified file.
[   ERROR   ][7/24/2022 11:23:09] Could not find file 'C:\Users\felix\Documents\projects\DefQed\DefQed\Examples\Diagnostic'.
```
This means the file you targeted cannot be accessed. There may be a typo in your parameters.

- If you encountered this type of JS failure:
```txt
DefQed. For detailed help, refer to github.
[INFORMATION][7/24/2022 11:27:57] OS: Microsoft Windows 10.0.25158
[INFORMATION][7/24/2022 11:27:57] Set LogLevel as    DEBUG   .
[  WARNING  ][7/24/2022 11:27:57] LoadJS feature is EXPERIMENTAL.
[INFORMATION][7/24/2022 11:27:57] Start to execute JavaScript.
[   ERROR   ][7/24/2022 11:27:57] Script execution error. There may be an error in the js file.
[   ERROR   ][7/24/2022 11:27:57] Details: SyntaxError: Unexpected identifier
```
There may be an error in your script file. Review that file.

- If you encountered a MySQL error like this:
```txt
DefQed. For detailed help, refer to github.
[INFORMATION][7/24/2022 11:29:49] OS: Microsoft Windows 10.0.25158
[INFORMATION][7/24/2022 11:29:49] Set LogLevel as    DEBUG   .
[  WARNING  ][7/24/2022 11:29:49] LoadJS feature is EXPERIMENTAL.
[INFORMATION][7/24/2022 11:29:50] Start to execute JavaScript.
[   ERROR   ][7/24/2022 11:29:50] Fatal error encountered when attempting to connect to database.
[   ERROR   ][7/24/2022 11:29:50] Authentication to host '127.0.0.1' for user 'DtefQed' using method 'sha256_password' failed with message: Access denied for user 'DtefQed'@'localhost' (using password: YES)
[   ERROR   ][7/24/2022 11:29:50] Connstr server=127.0.0.1;uid=DtefQed;pwd=oClg2%[TenbL86V+rsC3;database=defqed failed.
[   ERROR   ][7/24/2022 11:29:51] Script execution error. There may be an error in the js file.
[   ERROR   ][7/24/2022 11:29:51] Details: Error: Connection must be valid and open.
```
First,  check if the MySQL service is running. Then, check if your connection specification in the `JSDriver.Connect()` function is valid. For instance, the username ought to be `DefQed` instead of `DetfQed` in the example above.

- Other issue. There may be a **bug** please check the **Known issues** section below. You are welcome to inspect and you can contribute to improve it.

### The JavaScript API of specifying the proof target
JavaScript makes things simple. The diagnostic example, if expressed in XML format, uses 31 non-comment lines. However, if use the new JavaScript form, only takes up 7 lines. Additionally, the JavaScript form is more flexible. You can even output to console if use the debug build, for example:
```JavaScript
Console.WriteLine("Some thing"); // Console API is exposed with ClearScript.
```

The example file have these few lines of code:
```JavaScript
// This file is an example of the JS statement format.
// EXPERIMENTAL FEATURE!
// Use the defqed diagnostic database for this

JSDriver.Connect("DtefQed", "oClg2%[TenbL86V+rsC3", "defqed");
JSDriver.Enroll("item", "x");
JSDriver.Enroll("item", "y");
JSDriver.Enroll("item", "z");

JSDriver.Left(JSDriver.MicroStatement(JSDriver.SymbolHolder(JSDriver.Symbol("x")), JSDriver.Notation("=="), JSDriver.SymbolHolder(JSDriver.Symbol("y"))));
JSDriver.Left(JSDriver.MicroStatement(JSDriver.SymbolHolder(JSDriver.Symbol("y")), JSDriver.Notation("=="), JSDriver.SymbolHolder(JSDriver.Symbol("z"))));

JSDriver.Right(JSDriver.MicroStatement(JSDriver.SymbolHolder(JSDriver.Symbol("x")), JSDriver.Notation("=="), JSDriver.SymbolHolder(JSDriver.Symbol("z"))));
```

The DefQed API is exposed using noun `JSDriver`. To begin with, the script connect to the MySQL database. Details of this function could be found in the **Modify the JavaScript file** section.

Next, the symbols to be utilized are enrolled to the `SymbolBank` using `JSDriver.Enroll`:
```JavaScript
JSDriver.Enroll("type", "name");
```

Then, micro statements are added to the two pools using these two APIs.
```JavaScript
JSDriver.Left(microStatement);	// Insert into LeftPool
JSDriver.Right(microStatement);	// Insert into RightPool
```

Each microStatement is generated with `JSDriver.MicroStatement` API:
```JavaScript
JSDriver.MicroStatement(bracket0, connector, bracket1);
```

The connector is a `Notation` specified with
```JavaScript
JSDriver.Notation("name");	// for example, JSDriver.Notation("==");
```

Each bracket can be generated with one of the following four methods:
```JavaScript
JSDriver.SymbolHolder(symbol);
JSDriver.NegatedHolder(bracket);
JSDriver.StatementHolder(microStatement);
JSDriver.BracketHolder(bracket0, connector, bracket1);
```

Finally, a symbol is generated by `JSDriver.Symbol` API via:
```JavaScript
JSDriver.Symbol("name");
```
**Note.** A symbol must be enrolled wit a valid notation before being used.

**Note.** The JavaScript API will be simplified in a recent commit and there will be several changes in the future.

## Known issues
- The `FTC (Free Transistor Combinator)` algorithm is not reliable in certain conditions.
- Currently many of the logics are not implemented.
- Currently it cannot serialize KBase correctly.

## Acknowledgements
DefQed takes the advantage of these technologies:
- .NET
- Newtonsoft.Json
- MySQL
- BouncyCastle
- McMaster.Extensions.CommandLineUtils
- Microsoft.ClearScript
- JavaScript
- Mathematica (for research, not used by algorithm)

## One more thing
**Feel free to fork but do NOT plagiarize.** I may write a paper about it some time later so you shouldn't just copy the project and claim your originality. However, if you add new ideas to it, it then becomes your project.
