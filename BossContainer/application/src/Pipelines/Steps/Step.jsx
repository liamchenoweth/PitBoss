import React, { useEffect, useState } from 'react';
import { Link } from "react-router-dom";
import { makeStyles } from '@material-ui/core/styles';

import Paper from '@material-ui/core/Paper';
import { Typography } from '@material-ui/core';

const width = 200;
const height = 75;
export { width as StepWidth, height as StepHeight };

const styles = makeStyles(theme => ({
    step: {
        width: width,
        height: height,
        position: "absolute",
        textAlign: "center"
    },
    border: {
        border: "#8888 2px solid",
        borderRadius: "4px 4px 0 0",
        borderBottom: 0,
        position: "absolute",
        top: 0,
        left: 0,
        width: "100%",
        height: "calc(100% - 4px)"
    },
    distributedStep: {
        position: "absolute",
        left: 0,
        top: 0
    },
    healthBar: {
        height: 5,
        width: "100%",
        bottom: 0,
        position: "absolute",
        borderRadius: "0 0 4px 4px"
    },
    path: {
        height: 10,
        position: "absolute",
        backgroundColor: "blue"
    }
}));

function Step(props)
{
    var classes = styles();
    return (
        <foreignObject x={props.x} y={props.y} width={width + (props.isDistributed ? 10 : 0)} height={height + (props.isDistributed ? 10 : 0)}
            style={{ paddingTop: (props.isDistributed ? 10 : 0), paddingLeft: (props.isDistributed ? 10 : 0)}}
        >
            <Link to={`/operations/${props.name}`}>
                {props.isDistributed && 
                    <Paper className={`${classes.step} ${classes.distributedStep}`}>
                        <div className={classes.border}/>
                        <div className={`${classes.healthBar}`} style={{ backgroundColor: (props.color || "grey") }}/>
                    </Paper>}
                <Paper className={classes.step}>
                    <div className={classes.border}/>
                    <Typography variant="h6">{props.name}</Typography>
                    <Typography variant="subtitle1">{props.children}</Typography>
                    <div className={`${classes.healthBar}`} style={{ backgroundColor: (props.color || "grey") }}/>
                </Paper>
            </Link>
        </foreignObject>
    )
}

export default Step;