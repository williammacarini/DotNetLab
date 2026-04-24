#!/bin/bash

# Uso: ./create-lab.sh <NNN-nome-do-lab> [tipo]
#
# Tipos disponíveis:
#   classlib  (padrão) — biblioteca de classes
#   api                — ASP.NET Core Web API (minimal)
#   console            — aplicação console
#   worker             — background worker service
#
# Exemplos:
#   ./create-lab.sh 003-event-sourcing
#   ./create-lab.sh 004-minimal-api api
#   ./create-lab.sh 005-worker-service worker

if [ -z "$1" ]; then
    echo "Uso: ./create-lab.sh <NNN-nome-do-lab> [tipo]"
    echo ""
    echo "Tipos disponíveis:"
    echo "  classlib  (padrão) — biblioteca de classes"
    echo "  api                — ASP.NET Core Web API"
    echo "  console            — aplicação console"
    echo "  worker             — background worker service"
    echo ""
    echo "Exemplos:"
    echo "  ./create-lab.sh 003-event-sourcing"
    echo "  ./create-lab.sh 004-minimal-api api"
    exit 1
fi

LAB_SLUG="$1"
TYPE="${2:-classlib}"

case "$TYPE" in
    classlib|api|console|worker) ;;
    *)
        echo "Tipo inválido: '$TYPE'"
        echo "Tipos válidos: classlib, api, console, worker"
        exit 1
        ;;
esac

LAB_DIR="labs/$LAB_SLUG"
LAB_NAME=$(echo "$LAB_SLUG" | sed 's/^[0-9]*-//')
LAB_PASCAL=$(echo "$LAB_NAME" | awk -F'-' '{for(i=1;i<=NF;i++) $i=toupper(substr($i,1,1)) substr($i,2); print}' OFS='')

SRC_PROJ="$LAB_DIR/src/$LAB_PASCAL/$LAB_PASCAL.csproj"
TEST_PROJ="$LAB_DIR/tests/$LAB_PASCAL.Tests/$LAB_PASCAL.Tests.csproj"
SLNX="$LAB_DIR/$LAB_PASCAL.slnx"

echo "Criando lab: $LAB_SLUG"
echo "  projeto: $LAB_PASCAL"
echo "  tipo:    $TYPE"
echo ""

mkdir -p "$LAB_DIR/src/$LAB_PASCAL"
mkdir -p "$LAB_DIR/tests/$LAB_PASCAL.Tests"

# Cria o projeto src conforme o tipo
case "$TYPE" in
    classlib)
        dotnet new classlib -n "$LAB_PASCAL" -o "$LAB_DIR/src/$LAB_PASCAL" --framework net10.0
        rm -f "$LAB_DIR/src/$LAB_PASCAL/Class1.cs"
        ;;
    api)
        dotnet new webapi -n "$LAB_PASCAL" -o "$LAB_DIR/src/$LAB_PASCAL" --framework net10.0
        ;;
    console)
        dotnet new console -n "$LAB_PASCAL" -o "$LAB_DIR/src/$LAB_PASCAL" --framework net10.0
        ;;
    worker)
        dotnet new worker -n "$LAB_PASCAL" -o "$LAB_DIR/src/$LAB_PASCAL" --framework net10.0
        ;;
esac

# Cria o projeto de testes
dotnet new xunit -n "$LAB_PASCAL.Tests" -o "$LAB_DIR/tests/$LAB_PASCAL.Tests" --framework net10.0
dotnet add "$TEST_PROJ" reference "$SRC_PROJ"

# Cria o .slnx do lab
cat > "$SLNX" <<EOF
<Solution>
  <Folder Name="/src/">
    <Project Path="src/$LAB_PASCAL/$LAB_PASCAL.csproj" />
  </Folder>
  <Folder Name="/tests/">
    <Project Path="tests/$LAB_PASCAL.Tests/$LAB_PASCAL.Tests.csproj" />
  </Folder>
</Solution>
EOF

# Adiciona ao .slnx raiz
dotnet sln DotNetLab.slnx add "$SRC_PROJ" "$TEST_PROJ"

echo ""
echo "Lab '$LAB_SLUG' criado em $LAB_DIR/"
echo ""

if [ "$TYPE" = "classlib" ]; then
    echo "  cd $LAB_DIR && dotnet test"
else
    echo "  cd $LAB_DIR && dotnet run --project src/$LAB_PASCAL"
    echo "  cd $LAB_DIR && dotnet test"
fi
