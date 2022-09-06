# ANT-APCT-Paper
LaTeX source for paper A new type of automated prover based on category theory.

## To compile
1. Clone the repository.
2. Compile with XELATEX:
```bash
$ xelatex.exe -synctex=1 -interaction=nonstopmode -output-driver='xdvipdfmx -z0' paper.tex
```