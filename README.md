# DotNetLab

Laboratório pessoal de experimentos .NET — patterns, algoritmos e bibliotecas.

## Estrutura

```
labs/
  NNN-nome-do-lab/
    src/NomeLab/         ← código do lab
    tests/NomeLab.Tests/ ← testes
    NomeLab.slnx         ← solução isolada (abre só esse lab)
DotNetLab.slnx           ← solução raiz (abre tudo)
Makefile
scripts/create-lab.sh
```

Cada lab é **completamente independente**: pode ser aberto, buildado e testado sem depender de nada fora da sua pasta.

---

## Labs disponíveis

| Lab | Conteúdo | Como rodar |
|-----|----------|------------|
| [001-saga-pattern](labs/001-saga-pattern/) | Saga Pattern com MassTransit (state machine + in-memory) | `make run LAB=001-saga-pattern` |
| [002-rate-limit](labs/002-rate-limit/) | 4 algoritmos de rate limiting (Fixed Window, Sliding Window, Token Bucket, Leaky Bucket) | `make test LAB=002-rate-limit` |

---

## Comandos

### Buildar e testar tudo

```bash
make build      # builda todos os labs
make test       # roda todos os testes
```

### Trabalhar em um lab específico

```bash
# Opção 1: via Makefile (da raiz)
make run  LAB=001-saga-pattern    # roda a API do lab
make test LAB=002-rate-limit      # testa só esse lab
make watch LAB=002-rate-limit     # watch tests (TDD)

# Opção 2: diretamente no diretório do lab
cd labs/002-rate-limit
dotnet test
dotnet build
```

---

## Criar um novo lab

```bash
make new-lab NAME=003-event-sourcing              # classlib (padrão)
make new-lab NAME=004-minimal-api   TYPE=api      # ASP.NET Core Web API
make new-lab NAME=005-background    TYPE=worker   # Background worker service
make new-lab NAME=006-cli-tool      TYPE=console  # Console app
```

| TYPE | Template .NET | Quando usar |
|------|---------------|-------------|
| `classlib` | `dotnet new classlib` | Algoritmos, patterns, bibliotecas |
| `api` | `dotnet new webapi` | APIs REST, minimal APIs |
| `console` | `dotnet new console` | Scripts, CLIs, demos rápidas |
| `worker` | `dotnet new worker` | Background jobs, hosted services |

O script `scripts/create-lab.sh` vai:
1. Criar `labs/NNN-nome/src/NomeLab/` com o template escolhido
2. Criar `labs/NNN-nome/tests/NomeLab.Tests/` com xUnit
3. Gerar o `NomeLab.slnx` do lab
4. Adicionar os projetos ao `DotNetLab.slnx` raiz

Depois é só abrir `labs/NNN-nome/NomeLab.slnx` no IDE e começar a explorar.

---

## Convenções

- **Número sequencial no nome** do diretório: `NNN-nome-descritivo`
- **Namespace livre**: use o que fizer sentido para o lab, sem acoplamento à raiz
- **Dependências isoladas**: cada lab declara só os pacotes que precisa no seu `.csproj`
- **README por lab**: documente o conceito, referências e como rodar no `README.md` do lab

---

## Requisitos

- .NET 10 SDK
- `make` (qualquer sistema)
