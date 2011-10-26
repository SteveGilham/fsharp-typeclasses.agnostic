﻿module Control.Monad.Cont

open Prelude
type Cont<'r,'a> = Cont of (('a->'r)->'r) with
    static member ( ? ) ( Cont m , _Functor:Fmap)                 = fun f -> Cont <| fun c -> m (c << f)

    static member (?<-) (_:Return, _Monad  :Return, t:Cont<_,'a>) = fun (n:'a) -> Cont (fun k -> k n)
    static member ( ? ) (Cont m  , _Monad  :Bind)                 =
        let runCont (Cont x) = x
        fun f -> Cont (fun k -> m (fun a -> runCont (f a) k))

let runCont (Cont x) = x

let callCC f = Cont <| fun k -> runCont (f (fun a -> Cont <| fun _ -> k a)) k

open Control.Monad.Trans

type ContT< ^rma> = ContT of ^rma with
    static member inline ( ? ) (ContT m , _Functor:Fmap              ) = fun f -> ContT <| fun c -> m (c << f)

    static member inline (?<-) (_:Return, _Monad  :Return, t:ContT<_>) = fun a -> ContT ((|>) a)
    static member inline ( ? ) (ContT m , _Monad  :Bind              ) =
        let inline runContT (ContT x) = x
        fun k -> ContT <| fun c -> m (fun a -> runContT (k a) c)

    static member inline (?<-) (m      , _MonadTrans:Lift, t:ContT<_>) = ContT ((>>=) m)

let mapContT      f (ContT m) = ContT (f << m)
let withContT     f (ContT m) = ContT (m << f)
let inline runContT (ContT x) = x