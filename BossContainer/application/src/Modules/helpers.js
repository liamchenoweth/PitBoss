import React from 'react';

import { makeStyles } from '@material-ui/core/styles';

import Healthy from '@material-ui/icons/CheckCircle';
import Warning from '@material-ui/icons/Warning';
import Unhealthy from '@material-ui/icons/Cancel';

const styles = makeStyles(theme => ({
    iconRotate: {
        transform: "rotate(-45deg)"
    }
}));

export function getHealthSymbol(text)
{ 
    var classes = styles(); 
    if(text === "Healthy") return <Healthy style={{color: "green"}}/>;
    if(text === "Warning") return <Warning style={{color: "orange"}}/>;
    if(text === "Unhealthy") return <Unhealthy style={{color: "red"}}/>;
}