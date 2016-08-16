module PlayerInput

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Actors

let rotationForce = 1.0
let force  = 25.0

let rec private HandleKeys keys actor body =
    match keys with
    | [] -> (actor, body)
    | k :: l -> 
                let HandleKeys' = HandleKeys l
                match k with
                | Keys.Left -> 
                    HandleKeys' actor {body with RotationAcceleration = -rotationForce}
                | Keys.Right -> 
                    HandleKeys' actor {body with RotationAcceleration = rotationForce}
                | Keys.Up -> 
                    HandleKeys' actor {body with Acceleration = Vector2.Transform(Vector2(0.f,float32 -force), Matrix.CreateRotationZ(float32 actor.Rotation))}
                | Keys.Down -> 
                    HandleKeys' actor {body with Acceleration = Vector2.Transform(Vector2(0.f,float32  force), Matrix.CreateRotationZ(float32 actor.Rotation))}
                | Keys.Space ->
                    let shotType = match actor.ActorType with
                                   | Player(s) -> s
                                   | _ -> {IsShooting = false ; Texture = null; LastShot = 0; ShotCooldown = 0}

                    HandleKeys' {actor with ActorType = Player({shotType with IsShooting = true})} body
                | _ -> 
                    HandleKeys' actor body

let HandleInput (kbState:KeyboardState) actor =
    match actor.ActorType with
    | Player(s) ->
        let initial = match actor.BodyType with
                      | Dynamic(d) -> d
                      | Static -> {Velocity = Vector2(0.f, 0.f) ; Acceleration = Vector2(0.f, 0.f) ; Friction = 0.0 ; RotationVelocity = 0.0 ; RotationAcceleration = 0.0 ; RotationFriction = 0.0}

        let hActor, hBody = HandleKeys(kbState.GetPressedKeys() |> Array.toList) actor initial
        {hActor with BodyType = Dynamic(hBody)}
    | _ -> actor