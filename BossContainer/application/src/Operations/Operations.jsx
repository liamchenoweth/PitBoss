import React, { useEffect, useState } from 'react';

import Typography from '@material-ui/core/Typography';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TablePagination from '@material-ui/core/TablePagination';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Switch, Route, Link, useLocation } from "react-router-dom";

import Operation from './Operation';

import Paginate from '../Utilities/Paginate/Paginate';
import { Get } from '../Modules/requests';
import { getHealthSymbol } from '../Modules/helpers';

function OperationRow(props)
{
    return (
        <TableRow>
            <TableCell><Link to={`operations/${props.name}`}>{props.name}</Link></TableCell>
            <TableCell>{getHealthSymbol(props.status.groupHealth)}</TableCell>
            <TableCell>{props.script}</TableCell>
            <TableCell>{props.currentSize}/{props.targetSize}</TableCell>
        </TableRow>
    )
}

function OperationList()
{
    let [operations, updateOperations] = useState([]);
    var [pageRows, updatePageRows] = useState(10);
    var [currentPage, updateCurrentPage] = useState(0);
    useEffect(() => {
        (async () => {
            var operationList = (await Get("/api/operations")).data;
            updateOperations(operationList);
        })();
    }, [])
    return(
        <div>
            <Typography variant="h4">Operations</Typography>
            <br/>
            <Typography variant="body1">
                These are the operation groups that serve your pipelines
            </Typography>
            <br/>
            <TableContainer component={"div"}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>Operation</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Script</TableCell>
                            <TableCell>Containers</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        <Paginate pageCount={pageRows} page={currentPage}>
                            {
                                operations.map(item => {
                                    return <OperationRow {...item} key={item.name}/>
                                })
                            }
                        </Paginate>
                        <TableRow>
                            <TablePagination 
                                rowsPerPageOptions={[10, 25, 50, 100]}
                                page={currentPage} 
                                rowsPerPage={pageRows} 
                                count={operations.length} 
                                onChangePage={(event, page) => {updateCurrentPage(page)}}
                                onChangeRowsPerPage={(event) => updatePageRows(event.target.value)}
                                />
                        </TableRow>
                    </TableBody>
                </Table>
            </TableContainer>
        </div>
    )
}

function Operations()
{
    return (
        <div>
            <Switch>
                <Route path="/operations/:name">
                    <Operation/>
                </Route>
                <Route exact path="/operations">
                    <OperationList/>
                </Route>
            </Switch>
        </div>
    )
}

export default Operations;