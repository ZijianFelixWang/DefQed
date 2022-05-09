# DefQed
![](https://img.shields.io/github/license/ZijianFelixWang/DefQed?style=flat-square)
![](https://img.shields.io/github/languages/code-size/ZijianFelixWang/DefQed?style=flat-square)
![](https://img.shields.io/tokei/lines/github/ZijianFelixWang/DefQed?style=flat-square)

## What's This?
**DefQed** is a statement prover with its own algorithm for academic written in C#. It is immature and still in active (and very early) development.

## What license does it use?
The BSD 3-Clause "New" or "Revised" License. For more information, see the `LICENSE` file.

The Mathematica notebook inside use a Creative Commons license. Open it for further information.

## What can it do?
Currently it can prove statements described in the input XML file with essential 'knowledge' installed in the database. What's special, it does **not** contain any reflections (i.e. rules) itself, even the simplest (x==y&&y==z)=>(x==z) ones.

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

For unimplemented ones, see `/DefQed/Documents/Roadmap.md`.

## Usage
For a quick intro, go to Get Started section.

### Requirements
The program is currently only tested in Windows. However, further support on Linux and macOS will be added.
- *.NET 6.0 runtime*: You only need the basic one as it's a console application.
- *MySQL Database*: The program store data in it so such is necessary.

### Releases
- `v0.01`: Experimental but basically working version. Only offer Windows binary in release page.

### Build
Load the `.sln` file in Visual Studio (recommend: version 2022) and perform build.

**Note.** Unless you want to develop the program, choose `Release` template instead of `Debug`. There are differences,

### Get Started
**Note.** You may want to add it to `PATH`: `DefQed.exe`

DefQed is a console program. Command line:
```PowerShell
./DefQed.exe file.xml
```
Wait... Before executing such, you need to configure the database. In MySQL, create a new database and a new user (and a password) for the program to use. Then create all of these tables:
```
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
```
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
```
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
```
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
**Note.** The format above is far from perfect. It will be changd later.

## Known issues
- The `FTC (Free Transistor Combinator)` algorithm is not reliable in certain conditions.
- Currently many of the logics are not implemented.
- Currently it cannot serialize KBase correctly.
- There are a lot of useless lines of code which impact code quality. (Will be removed soon)

## Acknowledgements
DefQed uses these technologies:
- .NET
- Newtonsoft.Json
- MySQL
- BouncyCastle
- Mathematica

## One more thing
**Feel free to fork but do NOT plagiarize.** I may write a paper about it some time later so you shouldn't just copy the project and claim your originality. However, if you add new ideas to it, it then becomes your project.
