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
        <foreignObject x={props.x} y={props.y} width={width} height={height}>
            <Link to={`/operations/${props.name}`}>
                <Paper className={classes.step}>
                    <Typography variant="h6">{props.name}</Typography>
                    <Typography variant="subtitle1">{props.children}</Typography>
                    <div className={`${classes.healthBar}`} style={{ backgroundColor: (props.color || "grey") }}/>
                </Paper>
            </Link>
        </foreignObject>
    )
}

export default Step;