import React, { useEffect, useState } from 'react';
import { Link } from "react-router-dom";

import Stop from '@material-ui/icons/Stop';

import Typography from '@material-ui/core/Typography';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableContainer from '@material-ui/core/TableContainer';
import TablePagination from '@material-ui/core/TablePagination';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Paginate from '../Utilities/Paginate/Paginate';

import { Get } from '../Modules/requests';

function RequestRow(props)
{
    return (
        <TableRow>
            <TableCell>{props.id}</TableCell>
            <TableCell><Link to={`/pipelines/${props.pipelineName}`}>{props.pipelineName}</Link></TableCell>
            <TableCell>{props.status}</TableCell>
            <TableCell>{props.input}</TableCell>
            {(props.status == "Pending" || props.status == "Executing") ? <TableCell><Stop style={{color: "red"}}/></TableCell> : <TableCell/>}
        </TableRow>
    )
}

function Requests()
{
    let [requests, updateRequests] = useState([]);
    var [pageRows, updatePageRows] = useState(10);
    var [currentPage, updateCurrentPage] = useState(0);
    useEffect(() => {
        (async () => {
            var requestList = (await Get("/api/requests")).data;
            updateRequests(requestList);
        })();
    }, [])
    return (
        <div>
            <Typography variant="h4">Requests</Typography>
            <br/>
            <Typography variant="body1">
                These are the requests made to your pipelines
            </Typography>
            <br/>
            <TableContainer component={"div"}>
                <Table size="small">
                    <TableHead>
                        <TableRow>
                            <TableCell>Id</TableCell>
                            <TableCell>Pipeline</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Input</TableCell>
                            <TableCell></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        <Paginate pageCount={pageRows} page={currentPage}>
                            {
                                requests.map(item => {
                                    return <RequestRow {...item} key={item.id}/>
                                })
                            }
                        </Paginate>
                        <TableRow>
                            <TablePagination 
                                rowsPerPageOptions={[10, 25, 50, 100]}
                                page={currentPage} 
                                rowsPerPage={pageRows} 
                                count={requests.length} 
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

export default Requests;