{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell{
  packages = [
    pkgs.dotnet-sdk
    pkgs.dotnet-runtime
    pkgs.dotnet-aspnetcore
    pkgs.dotnet-repl
  ];
  shellHook = ''
    export SHELL=$(which zsh)
    nvim && exit
  '';
}
