.PHONY: all build test clean

all: build test clean

build:
	dotnet restore
	dotnet build --configuration Release --no-restore
	dotnet test --no-restore --verbosity normal

test:
	dotnet test

clean:
	dotnet clean
	rm -rf ./src/*/*.db
	rm -rf ./src/*/{bin,obj}
	rm -rf ./tests/*/{bin,obj}
