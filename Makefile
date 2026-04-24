.PHONY: build test run clean watch new-lab

# Uso: make run LAB=001-saga-pattern
LAB  ?=
TYPE ?= classlib

build:
	dotnet build

test:
	dotnet test --nologo

# Roda um lab específico: make run LAB=001-saga-pattern
run:
	@if [ -z "$(LAB)" ]; then echo "Informe o lab: make run LAB=001-saga-pattern"; exit 1; fi
	dotnet run --project labs/$(LAB)/src

clean:
	dotnet clean

# Observa testes de um lab: make watch LAB=002-rate-limit
watch:
	@if [ -z "$(LAB)" ]; then echo "Informe o lab: make watch LAB=002-rate-limit"; exit 1; fi
	dotnet watch test --project labs/$(LAB)/tests

# Cria um novo lab: make new-lab NAME=003-event-sourcing [TYPE=api|console|worker|classlib]
new-lab:
	@if [ -z "$(NAME)" ]; then echo "Informe o nome: make new-lab NAME=003-event-sourcing [TYPE=api]"; exit 1; fi
	@bash scripts/create-lab.sh $(NAME) $(TYPE)
