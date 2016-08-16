module Main

open System
open AsteroidGame
//open Flappy

[<EntryPoint>]
let main args = 
    let game = new AsteroidGame()
    game.Run()
    0