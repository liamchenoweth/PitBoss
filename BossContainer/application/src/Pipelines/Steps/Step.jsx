import React, { useEffect, useState } from 'react';

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
    healthy: {
        backgroundColor: "#7cb342"
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
            <Paper className={classes.step}>
                <Typography variant="h6">{props.name}</Typography>
                <Typography variant="subtitle1">24%</Typography>
                <div className={`${classes.healthBar} ${classes.healthy}`}/>
            </Paper>
        </foreignObject>
    )
}

export default Step;